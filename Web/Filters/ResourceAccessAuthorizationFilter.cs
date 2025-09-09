using System.Diagnostics.CodeAnalysis;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinaSys.Web.Filters;

/// <summary>
/// Authorization filter that validates user access to business incubators and projects.
/// Uses parameter naming conventions to automatically detect resource IDs.
/// Works in conjunction with IAccessChecker service used by CommandAuthorizationBehavior.
/// </summary>
/// <param name="businessIncubatorIdParam">
/// Parameter name for business incubator ID (convention-based).
/// </param>
/// <param name="projectIdParam">
/// Parameter name for project ID (convention-based).
/// </param>
/// <param name="bypassRoles">
/// Specific roles that bypass resource checks (like GlobalAdministrator).
/// </param>
public class ResourceAccessAuthorizationFilter(
    IAccessChecker accessChecker,
    IAuthScopeProvider authScopeProvider,
    ILogger<ResourceAccessAuthorizationFilter> logger,
    string? businessIncubatorIdParam = null,
    string? projectIdParam = null,
    string[]? bypassRoles = null) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!authScopeProvider.IsAuthenticated())
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = authScopeProvider.GetCurrentUserId();
        var userRoles = authScopeProvider.GetCurrentUserRoles();

        if (bypassRoles != null && bypassRoles.Any(role => userRoles.Contains(role)))
        {
            logger.LogDebug("User {UserId} has bypass role, skipping resource access checks", userId);
            return;
        }

        if (!await TryValidateProjectAccessAsync(context, projectIdParam, userId))
        {
            return;
        }

        await TryValidateBusinessIncubatorAccessAsync(context, businessIncubatorIdParam, userId);
    }

    private static T GetParameterValue<T>(AuthorizationFilterContext context, string parameterName)
    {
        // Try to get from route values first
        if (context.RouteData.Values.TryGetValue(parameterName, out var routeValue))
        {
            if (TryConvertValue<T>(routeValue, out var convertedRouteValue))
            {
                return convertedRouteValue;
            }
        }

        // Try to get from action arguments
        if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor
            actionDescriptor)
        {
            var parameter = actionDescriptor.Parameters
                .FirstOrDefault(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));

            if (parameter != null && context.HttpContext.Request.RouteValues.TryGetValue(parameterName, out var value))
            {
                if (TryConvertValue<T>(value, out var convertedValue))
                {
                    return convertedValue;
                }
            }
        }

        return default!;
    }

    private static bool TryConvertValue<T>(object? value, out T result)
    {
        result = default!;

        if (value == null)
        {
            return false;
        }

        try
        {
            if (typeof(T) == typeof(Guid))
            {
                if (Guid.TryParse(value.ToString(), out var guidValue))
                {
                    result = (T)(object)guidValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(long))
            {
                if (long.TryParse(value.ToString(), out var longValue))
                {
                    result = (T)(object)longValue;
                    return true;
                }
            }
            else
            {
                result = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }
        }
        catch
        {
            // Conversion failed
        }

        return false;
    }

    private static bool TryFindBusinessIncubatorIdParameter(AuthorizationFilterContext context,
                string? customIncubatorIdParamName, [NotNullWhen(true)] out string? paramFound, out Type? paramType)
    {
        string[] paramNames = customIncubatorIdParamName != null
            ? [customIncubatorIdParamName]
            : ["businessIncubatorId", "incubatorId", "businessIncubatorExternalId", "incubatorExternalId"];

        return TryFindParameterAndType(context, paramNames, out paramFound, out paramType);
    }

    private static bool TryFindParameterAndType(AuthorizationFilterContext context, string[] paramNames,
        [NotNullWhen(true)] out string? paramFound, out Type? paramType)
    {
        paramFound = null;
        paramType = null;

        if (context.ActionDescriptor is not Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor actionDescriptor)
        {
            return false;
        }

        foreach (var name in paramNames)
        {
            var parameter = actionDescriptor.Parameters
                .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (parameter != null)
            {
                paramFound = name;
                paramType = parameter.ParameterType;
                return true;
            }
        }

        return false;
    }

    private static bool TryDetectProjectIdParameter(AuthorizationFilterContext context,
            string? customProjectIdParamName, [NotNullWhen(true)] out string? paramFound, out Type? paramType)
    {
        string[] paramNames = customProjectIdParamName != null
            ? [customProjectIdParamName]
            : ["projectId", "projectExternalId"];

        return TryFindParameterAndType(context, paramNames, out paramFound, out paramType);
    }

    private async Task<bool> TryValidateBusinessIncubatorAccessAsync(AuthorizationFilterContext context,
        string? customIncubatorIdParamName, string userId)
    {
        if (!TryFindBusinessIncubatorIdParameter(context, customIncubatorIdParamName, out var paramName, out var paramType))
        {
            return true;
        }

        // Use the detected type to get the parameter value directly
        if (paramType == typeof(Guid))
        {
            var guidId = GetParameterValue<Guid>(context, paramName);
            if (guidId == Guid.Empty)
            {
                logger.LogWarning("Could not extract business incubator GUID from parameter {ParamName}", paramName);
                context.Result = new BadRequestResult();
                return false;
            }

            var hasAccess = await accessChecker.HasBusinessIncubatorAccessAsync(userId, guidId, context.HttpContext.RequestAborted);
            if (hasAccess)
            {
                return true;
            }

            logger.LogWarning("User {UserId} lacks access to business incubator {IncubatorId}", userId, guidId);
            context.Result = new ForbidResult();
            return false;
        }
        else if (paramType == typeof(long))
        {
            var longId = GetParameterValue<long>(context, paramName);
            if (longId == 0)
            {
                logger.LogWarning("Could not extract business incubator long ID from parameter {ParamName}", paramName);
                context.Result = new BadRequestResult();
                return false;
            }

            var hasAccess = await accessChecker.HasBusinessIncubatorAccessAsync(userId, longId, context.HttpContext.RequestAborted);
            if (hasAccess)
            {
                return true;
            }

            logger.LogWarning("User {UserId} lacks access to business incubator {IncubatorId}", userId, longId);
            context.Result = new ForbidResult();
            return false;
        }

        logger.LogWarning("Unsupported parameter type {ParamType} for business incubator ID parameter {ParamName}", paramType?.Name ?? "unknown", paramName);
        context.Result = new BadRequestResult();
        return false;
    }

    private async Task<bool> TryValidateProjectAccessAsync(AuthorizationFilterContext context,
        string? customProjectIdParamName, string userId)
    {
        if (!TryDetectProjectIdParameter(context, customProjectIdParamName, out var detectedParamName, out var paramType))
        {
            return true;
        }

        if (paramType == typeof(Guid))
        {
            var guidId = GetParameterValue<Guid>(context, detectedParamName);
            if (guidId == Guid.Empty)
            {
                logger.LogWarning("Could not extract project GUID from parameter {ParamName}", detectedParamName);
                context.Result = new BadRequestResult();
                return false;
            }

            var hasAccess = await accessChecker.HasProjectAccessAsync(userId, guidId, context.HttpContext.RequestAborted);
            if (hasAccess)
            {
                return true;
            }

            logger.LogWarning("User {UserId} lacks access to project {ProjectId}", userId, guidId);
            context.Result = new ForbidResult();
            return false;
        }

        if (paramType == typeof(long))
        {
            var longId = GetParameterValue<long>(context, detectedParamName);
            if (longId == 0)
            {
                logger.LogWarning("Could not extract project long ID from parameter {ParamName}", detectedParamName);
                context.Result = new BadRequestResult();
                return false;
            }

            var hasAccess = await accessChecker.HasProjectAccessAsync(userId, longId, context.HttpContext.RequestAborted);
            if (hasAccess)
            {
                return true;
            }

            logger.LogWarning("User {UserId} lacks access to project {ProjectId}", userId, longId);
            context.Result = new ForbidResult();
            return false;
        }

        logger.LogWarning("Unsupported parameter type {ParamType} for project ID parameter {ParamName}", paramType?.Name ?? "unknown", detectedParamName);
        context.Result = new BadRequestResult();
        return false;
    }

    /// <summary>
    /// Attribute to apply resource access authorization to controllers or actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ResourceAccessAuthorizationAttribute : Attribute, IFilterFactory
    {
        /// <summary>
        /// Gets or sets the parameter name for business incubator ID.
        /// If not set, will use convention-based detection.
        /// </summary>
        public string? BusinessIncubatorIdParam { get; set; }

        /// <summary>
        /// Gets or sets roles that bypass resource checks (e.g., "GlobalAdministrator").
        /// </summary>
        public string[]? BypassForRoles { get; set; } = [Roles.GlobalAdministrator];

        public bool IsReusable => false;

        /// <summary>
        /// Gets or sets the parameter name for project ID.
        /// If not set, will use convention-based detection.
        /// </summary>
        public string? ProjectIdParam { get; set; }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var accessChecker = serviceProvider.GetRequiredService<IAccessChecker>();
            var authScopeProvider = serviceProvider.GetRequiredService<IAuthScopeProvider>();
            var logger = serviceProvider.GetRequiredService<ILogger<ResourceAccessAuthorizationFilter>>();

            return new ResourceAccessAuthorizationFilter(
                accessChecker,
                authScopeProvider,
                logger,
                BusinessIncubatorIdParam,
                ProjectIdParam,
                BypassForRoles);
        }
    }
}
