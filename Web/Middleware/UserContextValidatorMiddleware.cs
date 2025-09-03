using LinaSys.Auth.Application.Commands.Context;
using LinaSys.Auth.Application.Queries.Context;
using MediatR;
using OpenTelemetry.Trace;

namespace LinaSys.Web.Middleware;

public class UserContextValidatorMiddleware(
    RequestDelegate next,
    IServiceScopeFactory scopeFactory,
    ILogger<UserContextValidatorMiddleware> logger)
{
    private static readonly HashSet<string> _staticFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".otf", ".eot", ".mp4", ".webm", ".json", ".txt", ".xml", ".webp",
    };

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsStaticFileRequest(context))
        {
            await next(context);
            return;
        }

        if (IsPublicWebFeature(context))
        {
            await next(context);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var tracer = scope.ServiceProvider.GetRequiredService<Tracer>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        using var span = tracer.StartActiveSpan(nameof(ProtectedResourcesAuthorizationMiddleware));

        var path = context.Request.Path.Value ?? string.Empty;

        // Skip context selection page itself to avoid redirect loop
        if (path.Contains("/ContextSelection", StringComparison.OrdinalIgnoreCase))
        {
            span.SetAttribute("usercontext.status", "Bypass since is the context selection controller to avoid redirect loop");
            await next(context);
            return;
        }

        // Skip password change page to allow users with temporary passwords to change them
        if (path.Contains("/Account/Manage/ChangePassword", StringComparison.OrdinalIgnoreCase))
        {
            span.SetAttribute("usercontext.status", "Bypass for password change page");
            await next(context);
            return;
        }

        // Get user ID from claims
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            // Not authenticated, let authorization middleware handle it
            span.SetAttribute("usercontext.status", "Not authenticated. This scenario shouldn't happen due to the pipelines configuration.");
            context.Response.Redirect(ProtectedResourcesAuthorizationMiddleware.LoginPath);
            return;
        }

        // Check if user has a valid context
        var hasValidContext = await IsValidUserContextAsync(userId, mediator);

        if (!hasValidContext)
        {
            span.SetAttribute("usercontext.status", "Invalid context. Redirected to select context.");
            logger.LogInformation("User {UserId} has invalid context, redirecting to context selection", userId);
            context.Response.Redirect("/ContextSelection");
            return;
        }

        // Context is valid, continue
        await next(context);
    }

    private static bool IsPublicWebFeature(HttpContext context)
    {
        if (context.Items.TryGetValue(ProtectedResourcesAuthorizationMiddleware.WebFeaturePublicKey, out var item))
        {
            if (item is not null)
            {
                return (bool)item;
            }
        }

        return false;
    }

    private static bool IsStaticFileRequest(HttpContext context)
    {
        var path = context.Request.Path;

        return path.HasValue && _staticFileExtensions.Contains(Path.GetExtension(path.Value));
    }

    private async Task<bool> IsValidUserContextAsync(string userId, IMediator mediator)
    {
        // Load user's saved context
        var loadCommand = new GetLastUserContextCommand(userId);
        var loadResult = await mediator.Send(loadCommand);

        if (!loadResult.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to load user context for user {userId}");
        }

        if (loadResult.Value == null)
        {
            return false;
        }

        var userContext = loadResult.Value;

        // Validate the loaded context
        var validateQuery = new ValidateUserContextQuery(userContext);
        var validateResult = await mediator.Send(validateQuery);

        return validateResult is { IsSuccess: true, Value: true };
    }
}
