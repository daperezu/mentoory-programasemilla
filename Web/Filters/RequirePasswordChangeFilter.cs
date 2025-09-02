using LinaSys.UserManagement.Application.Queries.GetUserProfileByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinaSys.Web.Filters;

/// <summary>
/// Action filter that enforces password change for users with temporary passwords.
/// </summary>
public class RequirePasswordChangeFilter(IMediator mediator, ILogger<RequirePasswordChangeFilter> logger) : IAsyncActionFilter
{
    // List of paths that should be allowed even when password change is required
    private static readonly HashSet<string> _allowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Identity/Account/Logout",
        "/Identity/Account/Manage/ChangePassword",
        "/Identity/Account/AccessDenied",
        "/Account/Manage/ChangePassword",  // Add the path without Identity prefix
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Skip check for unauthenticated users
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            await next();
            return;
        }

        // Get current path
        var path = context.HttpContext.Request.Path.Value ?? string.Empty;

        // Log the current path for debugging
        logger.LogDebug("RequirePasswordChangeFilter checking path: {Path}", path);

        // Skip check for allowed paths
        if (_allowedPaths.Any(allowed => path.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogDebug("Path {Path} is in allowed list, skipping password change check", path);
            await next();
            return;
        }

        // Skip for static files and resources
        var extension = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(extension) ||
            path.Contains("/css/") || path.Contains("/js/") || path.Contains("/lib/") ||
            path.Contains("/img/") || path.Contains("/assets/") ||
            path.Contains("/vendors/") || path.Contains("/_framework/") || path.Contains("/_content/"))
        {
            logger.LogDebug("Path {Path} is a static resource, skipping password change check", path);
            await next();
            return;
        }

        // Get user ID from claims
        var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            await next();
            return;
        }

        // Check if user requires password change
        try
        {
            var query = new GetUserProfileByUserIdQuery(userIdClaim.Value);
            var profileResult = await mediator.Send(query);

            if (profileResult is { IsSuccess: true, Value: not null })
            {
                var passwordChangePref = profileResult.Value.Preferences
                    ?.FirstOrDefault(p => p.Key == "auth.requires_password_change");
                var requiresPasswordChange = passwordChangePref?.Value == "true";

                if (requiresPasswordChange)
                {
                    logger.LogInformation("User {UserId} requires password change, redirecting from {Path}",
                        userIdClaim.Value, path);

                    // Redirect to change password page
                    context.Result = new RedirectToPageResult("/Account/Manage/ChangePassword",
                        new { area = "Identity", enforced = true });
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking password change requirement for user {UserId}", userIdClaim.Value);
        }

        await next();
    }
}
