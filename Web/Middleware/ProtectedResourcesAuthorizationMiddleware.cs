using System.Security.Claims;
using LinaSys.Auth.Application.Queries;
using LinaSys.Permissions.Application.ProtectedResource.Queries;
using LinaSys.SystemFeatures.Application.Queries;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using OpenTelemetry.Trace;

namespace LinaSys.Web.Middleware;

public class ProtectedResourcesAuthorizationMiddleware(
    RequestDelegate next,
    IServiceScopeFactory scopeFactory,
    IMemoryCache cache,
    ILogger<ProtectedResourcesAuthorizationMiddleware> logger)
{
    public const string LoginPath = "/Identity/Account/Login";
    public const string WebFeaturePublicKey = "WebFeaturePublic";
    private const string AccessDeniedPath = "/Home/AccessDenied";
    private const string GlobalAdminRoleName = "Global Administrator";

    private static readonly HashSet<string> _staticFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".otf", ".eot", ".mp4", ".webm", ".json", ".txt", ".xml", ".webp",
    };

    public enum UserProtectedResourceResult
    {
        AccessGranted,
        InvalidProtectedResourceId,
        NoAccess,
        ProtectedResourceNotRegistered,
    }

    public async Task Invoke(HttpContext context)
    {
        if (IsStaticFileRequest(context))
        {
            await next(context);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var tracer = scope.ServiceProvider.GetRequiredService<Tracer>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        using var span = tracer.StartActiveSpan(nameof(ProtectedResourcesAuthorizationMiddleware));

        var requestPath = context.Request.Path.Value;
        var routeData = context.GetRouteData();
        var (area, controller, action) = ExtractRouteDescriptor(routeData);
        var protectedResourceIds = ExtractProtectedResourceIds(routeData);

        ArgumentException.ThrowIfNullOrWhiteSpace(requestPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        var webFeatureInternalId = await IsWebFeaturePublicOrRequiresAuthorization(area, controller, action, requestPath, mediator, span);
        switch (webFeatureInternalId)
        {
            case 0: // Public Access
                context.Items[WebFeaturePublicKey] = true;
                await next(context);
                return;
            case -1: // Failed Authorization or other internal settings
                RedirectToAccessDenied(context);
                return;
        }

        var authenticatedUser = await IsUserAuthenticatedAndRolesAssignedAsync(context.User, requestPath, mediator, span);
        if (!authenticatedUser.HasValue)
        {
            RedirectToAccessDenied(context);
            return;
        }

        var userId = authenticatedUser.Value.UserId;
        var userRoles = authenticatedUser.Value.Roles;

        if (userRoles.Contains(GlobalAdminRoleName))
        {
            span.SetAttribute("authorization.status", $"Access Granted to {userId} : {GlobalAdminRoleName}");
            await next(context);
            return;
        }

        var hasAccessToWebFeature = await DemandUserHasAccessToProtectedResource(userId, userRoles, webFeatureInternalId, requestPath, mediator, span);

        if (!hasAccessToWebFeature)
        {
            RedirectToAccessDenied(context);
            return;
        }

        var hasAccessToProtectedResource = await UserHasAccessToProtectedResources(userId, userRoles, protectedResourceIds, requestPath, mediator, span);
        if (hasAccessToProtectedResource != UserProtectedResourceResult.AccessGranted)
        {
            RedirectToAccessDenied(context);
            return;
        }

        span.SetAttribute("authorization.status", "Access Granted");

        await next(context);
    }

    private static List<string> ExtractProtectedResourceIds(RouteData routeData)
    {
        var resourceIds = new List<string>();

        // Check for common route parameter names that might contain protected resource IDs
        var resourceParameterNames = new[] { "id", "businessIncubatorId", "projectId", "formId" };

        foreach (var parameterName in resourceParameterNames)
        {
            if (routeData.Values.TryGetValue(parameterName, out var value) && value?.ToString() is { } stringValue && !string.IsNullOrEmpty(stringValue))
            {
                resourceIds.Add(stringValue);
            }
        }

        return resourceIds;
    }

    private static (string Area, string Controller, string Action) ExtractRouteDescriptor(RouteData routeData)
    {
        var area = routeData.Values["area"]?.ToString() ?? string.Empty;
        var controller = routeData.Values["controller"]?.ToString() ?? "Home";
        var action = routeData.Values["action"]?.ToString() ?? "Index";

        if (!routeData.Values.TryGetValue("page", out var pageValue))
        {
            return (area, controller, action);
        }

        var pageTokens = ((string)pageValue!).Split("/", StringSplitOptions.RemoveEmptyEntries);

        controller = pageTokens[0];
        action = pageTokens[1];

        return (area, controller, action);
    }

    private static bool IsStaticFileRequest(HttpContext context)
    {
        var path = context.Request.Path;

        return path.HasValue && _staticFileExtensions.Contains(Path.GetExtension(path.Value));
    }

    private static void RedirectToAccessDenied(HttpContext context)
    {
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            context.Response.Redirect(AccessDeniedPath);
        }
        else
        {
            context.Response.Redirect($"{LoginPath}?ReturnUrl=" + Uri.EscapeDataString(context.Request.Path));
        }
    }

    private async Task<bool> DemandUserHasAccessToProtectedResource(
        string userId,
        IReadOnlyList<string> roles,
        long protectedResourceInternalId,
        string requestPath,
        IMediator mediator,
        TelemetrySpan span)
    {
        var hasRoleAccess = await mediator.Send(new RoleHasAccessToProtectedResourceQuery(roles.ToList(), protectedResourceInternalId));
        if (!hasRoleAccess.IsFailure)
        {
            var hasUserAccess = await mediator.Send(new UserHasAccessToProtectedResourceQuery(userId, protectedResourceInternalId));
            if (!hasUserAccess.IsFailure)
            {
                logger.LogWarning("Forbidden Access Attempt: {RequestPath} - User {UserId} - ProtectedResource {ProtectedResource} - None Role or User denial", requestPath, protectedResourceInternalId, userId);
                span.SetAttribute("authorization.status", "Forbidden - None Role Or User Denial");
                return false;
            }
        }

        return true;
    }

    private async Task<(string UserId, IReadOnlyList<string> Roles)?> IsUserAuthenticatedAndRolesAssignedAsync(
        ClaimsPrincipal user,
        string requestPath,
        IMediator mediator,
        TelemetrySpan span)
    {
        if (user.Identity is null)
        {
            logger.LogWarning("Invalid identity: {RequestPath}", requestPath);
            span.SetAttribute("authorization.status", "Not valid identity");
            return null;
        }

        if (!user.Identity.IsAuthenticated)
        {
            logger.LogWarning("Unauthorized Access Attempt: {RequestPath}", requestPath);
            span.SetAttribute("authorization.status", "Unauthorized");
            return null;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Unauthorized Access Attempt: {RequestPath} - User ID missing", requestPath);
            span.SetAttribute("authorization.status", "Unauthorized - Missing User ID");
            return null;
        }

        var result = await cache.GetOrCreateAsync($"{nameof(GetUserRolesQuery)}_{userId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await mediator.Send(new GetUserRolesQuery(userId));
        });

        var userRoles = result!.Value;

        if (userRoles is null || userRoles.Count == 0)
        {
            logger.LogWarning("Unauthorized Access Attempt: {RequestPath} - User doesn't have roles assigned", requestPath);
            span.SetAttribute("authorization.status", "Unauthorized - No roles assigned");
            return null;
        }

        return (userId, userRoles);
    }

    private async Task<long> IsWebFeaturePublicOrRequiresAuthorization(
        string area,
        string controller,
        string action,
        string requestPath,
        IMediator mediator,
        TelemetrySpan span)
    {
        var getWebFeatureByAreaControllerAndActionQueryKey = $"{nameof(GetWebFeatureByAreaControllerAndActionQuery)}_{action}_{controller}_{area}";
        var webFeature = await cache.GetOrCreateAsync(getWebFeatureByAreaControllerAndActionQueryKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var result = await mediator.Send(new GetWebFeatureByAreaControllerAndActionQuery(area, controller, action));
            return result.IsSuccess ? result.Value : null;
        });

        if (webFeature is null)
        {
            cache.Remove(getWebFeatureByAreaControllerAndActionQueryKey);
            logger.LogWarning("Unauthorized Access Attempt: {RequestPath}", requestPath);
            span.SetAttribute("authorization.status", "Feature not found in our DB. It might exists in the MVC but is not registered yet in the SystemFeatures module.");
            return -1;
        }

        if (webFeature.IsPublic)
        {
            logger.LogInformation("Public Access: {RequestPath}", requestPath);
            span.SetAttribute("authorization.status", "Public Access");
            return 0;
        }

        var getProtectedResourceByExternalIdQueryKey = $"{nameof(GetProtectedResourceByExternalIdQuery)}_{webFeature.ExternalId}";
        var protectedWebFeature = await cache.GetOrCreateAsync(getProtectedResourceByExternalIdQueryKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var result = await mediator.Send(new GetProtectedResourceByExternalIdQuery(webFeature.ExternalId));
            return result.IsSuccess ? result.Value : null;
        });

        if (protectedWebFeature is null)
        {
            cache.Remove(getProtectedResourceByExternalIdQueryKey);

            logger.LogWarning("Access Attempt: {RequestPath} - WebFeature {WebFeatureId} - GetProtectedResourceByExternalIdQuery failed", requestPath, webFeature.ExternalId);
            span.SetAttribute("authorization.status", "Forbidden - GetProtectedResourceByExternalIdQuery Denial");
            return -1;
        }

        return protectedWebFeature.InternalId;
    }

    private async Task<UserProtectedResourceResult> UserHasAccessToProtectedResources(
                string userId,
        IReadOnlyList<string> roles,
        List<string> protectedResourceIds,
        string requestPath,
        IMediator mediator,
        TelemetrySpan span)
    {
        if (protectedResourceIds.Count == 0)
        {
            return 0; // No protected resources in the request
        }

        // Check access to all protected resources in the route
        // For hierarchical routes, user must have access to ALL resources
        foreach (var protectedResourceId in protectedResourceIds)
        {
            if (Guid.TryParse(protectedResourceId, out var protectedResourceExternalIdAsGuid))
            {
                var result = await cache.GetOrCreateAsync($"{nameof(GetProtectedResourceByExternalIdQuery)}_{protectedResourceId}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await mediator.Send(new GetProtectedResourceByExternalIdQuery(protectedResourceExternalIdAsGuid));
                });

                var protectedResource = result!.IsSuccess ? result.Value : null;

                if (protectedResource is null)
                {
                    logger.LogWarning("Access Attempt: {RequestPath} - Protected Resource {ProtectedResource} - ExternalId not registered in ProtectedResources", requestPath, protectedResourceId);
                    span.SetAttribute("authorization.status", "Forbidden - ProtectedResources Denial");
                    return UserProtectedResourceResult.ProtectedResourceNotRegistered; // Protected Resource not registered
                }

                var hasAccessToProtectedResource = await DemandUserHasAccessToProtectedResource(userId, roles, protectedResource.InternalId, requestPath, mediator, span);
                if (!hasAccessToProtectedResource)
                {
                    return UserProtectedResourceResult.NoAccess; // User doesn't have access to this resource
                }
            }
            else
            {
                logger.LogInformation("Access Attempt: {RequestPath} - User {UserId} - Invalid External Protected Resource {ProtectedResourceExternalId} ", requestPath, userId, protectedResourceId);
                span.SetAttribute("authorization.status", "Forbidden - ProtectedResources Denial");
                return UserProtectedResourceResult.InvalidProtectedResourceId; // Invalid External Protected Resource
            }
        }

        return UserProtectedResourceResult.AccessGranted; // User has access to all required resources
    }
}
