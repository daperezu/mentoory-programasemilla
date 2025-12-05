using System.Diagnostics.CodeAnalysis;
using LinaSys.Auth.Application.Commands.Context;
using LinaSys.Auth.Application.Queries.Context;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Auth;
using LinaSys.Web.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinaSys.Web.Filters;

/// <summary>
/// Authorization filter that validates user context and resource access.
/// Assumes authentication is already handled by [Authorize] attribute.
/// Combines functionality from UserContextValidatorMiddleware and ResourceAccessAuthorizationFilter.
/// </summary>
public class UserContextAuthorizationFilter(
    IAccessChecker accessChecker,
    IAuthScopeProvider authScopeProvider,
    IMediator mediator,
    ILogger<UserContextAuthorizationFilter> logger,
    bool requireContext = true,
    bool requireIncubator = false,
    bool requireProject = false,
    string? businessIncubatorIdParam = null,
    string? projectIdParam = null,
    string[]? bypassRoles = null) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Authentication is already handled by [Authorize] attribute on AuthorizedBaseController
        // We can safely assume the user is authenticated at this point
        var userId = authScopeProvider.GetCurrentUserId();
        var userRoles = authScopeProvider.GetCurrentUserRoles();

        // Step 1: Check bypass roles (e.g., GlobalAdministrator)
        if (bypassRoles != null && bypassRoles.Any(role => userRoles.Contains(role)))
        {
            logger.LogDebug("User {UserId} has bypass role, skipping context and resource checks", userId);
            return;
        }

        // Step 2: Check if this is a special bypass path
        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        if (ShouldBypassContextCheck(path))
        {
            logger.LogDebug("Bypassing context check for path: {Path}", path);
            return;
        }

        // Step 3: Validate user context if required
        if (requireContext || requireIncubator || requireProject)
        {
            var hasValidContext = await ValidateUserContextAsync(
                userId,
                requireIncubator,
                requireProject,
                context.HttpContext.RequestAborted);

            if (!hasValidContext)
            {
                logger.LogInformation("User {UserId} has invalid or incomplete context, redirecting to context selection", userId);
                context.Result = new RedirectResult(ContextSelectionController.IndexUrl);
                return;
            }
        }

        // Step 4: Validate project access if project parameter is present
        if (!await TryValidateProjectAccessAsync(context, projectIdParam, userId))
        {
            return;
        }

        // Step 5: Validate business incubator access if parameter is present
        await TryValidateBusinessIncubatorAccessAsync(context, businessIncubatorIdParam, userId);
    }

    private static bool ShouldBypassContextCheck(string path)
    {
        // Skip context selection page itself to avoid redirect loop
        if (path.Contains("/ContextSelection", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Skip password change page to allow users with temporary passwords to change them
        if (path.Contains("/Account/Manage/ChangePassword", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
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

        // Try to get from query string
        if (context.HttpContext.Request.Query.TryGetValue(parameterName, out var queryValue))
        {
            var queryStringValue = queryValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(queryStringValue))
            {
                if (TryConvertValue<T>(queryStringValue, out var convertedQueryValue))
                {
                    return convertedQueryValue;
                }
            }
        }

        // Try to get from action arguments (for model binding scenarios)
        if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor actionDescriptor)
        {
            var parameter = actionDescriptor.Parameters
                .FirstOrDefault(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));

            if (parameter != null)
            {
                // Check if it's in the route values (already checked above, but being thorough)
                if (context.HttpContext.Request.RouteValues.TryGetValue(parameterName, out var value))
                {
                    if (TryConvertValue<T>(value, out var convertedValue))
                    {
                        return convertedValue;
                    }
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

    private static bool TryFindBusinessIncubatorIdParameter(
        AuthorizationFilterContext context,
        string? customIncubatorIdParamName,
        [NotNullWhen(true)] out string? paramFound,
        out Type? paramType)
    {
        string[] paramNames = customIncubatorIdParamName != null
            ? [customIncubatorIdParamName]
            : ["businessIncubatorId", "incubatorId", "businessIncubatorExternalId", "incubatorExternalId"];

        return TryFindParameterAndType(context, paramNames, out paramFound, out paramType);
    }

    private static bool TryDetectProjectIdParameter(
        AuthorizationFilterContext context,
        string? customProjectIdParamName,
        [NotNullWhen(true)] out string? paramFound,
        out Type? paramType)
    {
        string[] paramNames = customProjectIdParamName != null
            ? [customProjectIdParamName]
            : ["projectId", "projectExternalId"];

        return TryFindParameterAndType(context, paramNames, out paramFound, out paramType);
    }

    private static bool TryFindParameterAndType(
        AuthorizationFilterContext context,
        string[] paramNames,
        [NotNullWhen(true)] out string? paramFound,
        out Type? paramType)
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

    private async Task<bool> ValidateUserContextAsync(
        string userId,
        bool requireIncubator,
        bool requireProject,
        CancellationToken cancellationToken)
    {
        // Load user's saved context
        var loadCommand = new GetLastUserContextCommand(userId);
        var loadResult = await mediator.Send(loadCommand, cancellationToken);

        if (!loadResult.IsSuccess || loadResult.Value == null)
        {
            return false;
        }

        var userContext = loadResult.Value;

        // Validate the loaded context
        var validateQuery = new ValidateUserContextQuery(userContext);
        var validateResult = await mediator.Send(validateQuery, cancellationToken);

        if (!validateResult.IsSuccess || !validateResult.Value)
        {
            return false;
        }

        // Check specific requirements
        if (requireProject && (userContext.ProjectId == null || userContext.IncubatorId == null))
        {
            logger.LogDebug("User {UserId} context missing required project", userId);
            return false;
        }

        if (requireIncubator && userContext.IncubatorId == null)
        {
            logger.LogDebug("User {UserId} context missing required incubator", userId);
            return false;
        }

        return true;
    }

    private async Task<bool> TryValidateProjectAccessAsync(
        AuthorizationFilterContext context,
        string? customProjectIdParamName,
        string userId)
    {
        if (!TryDetectProjectIdParameter(context, customProjectIdParamName, out var detectedParamName, out var paramType))
        {
            return true; // No project parameter found, skip validation
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
            if (!hasAccess)
            {
                logger.LogWarning("User {UserId} lacks access to project {ProjectId}", userId, guidId);
                context.Result = new ForbidResult();
                return false;
            }
        }
        else if (paramType == typeof(long))
        {
            var longId = GetParameterValue<long>(context, detectedParamName);
            if (longId == 0)
            {
                logger.LogWarning("Could not extract project long ID from parameter {ParamName}", detectedParamName);
                context.Result = new BadRequestResult();
                return false;
            }

            var hasAccess = await accessChecker.HasProjectAccessAsync(userId, longId, context.HttpContext.RequestAborted);
            if (!hasAccess)
            {
                logger.LogWarning("User {UserId} lacks access to project {ProjectId}", userId, longId);
                context.Result = new ForbidResult();
                return false;
            }
        }

        return true;
    }

    private async Task<bool> TryValidateBusinessIncubatorAccessAsync(
        AuthorizationFilterContext context,
        string? customIncubatorIdParamName,
        string userId)
    {
        if (!TryFindBusinessIncubatorIdParameter(context, customIncubatorIdParamName, out var paramName, out var paramType))
        {
            return true; // No incubator parameter found, skip validation
        }

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
            if (!hasAccess)
            {
                logger.LogWarning("User {UserId} lacks access to business incubator {IncubatorId}", userId, guidId);
                context.Result = new ForbidResult();
                return false;
            }
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
            if (!hasAccess)
            {
                logger.LogWarning("User {UserId} lacks access to business incubator {IncubatorId}", userId, longId);
                context.Result = new ForbidResult();
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Attribute to apply unified user context and resource authorization to controllers or actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class UserContextAuthorizationAttribute : Attribute, IFilterFactory
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether to require a valid user context (default: true).
        /// </summary>
        public bool RequireContext { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether to require an incubator in the context.
        /// </summary>
        public bool RequireIncubator { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether to require a project in the context.
        /// </summary>
        public bool RequireProject { get; set; } = false;

        /// <summary>
        /// Gets or sets the parameter name for business incubator ID.
        /// If not set, will use convention-based detection.
        /// </summary>
        public string? BusinessIncubatorIdParam { get; set; }

        /// <summary>
        /// Gets or sets the parameter name for project ID.
        /// If not set, will use convention-based detection.
        /// </summary>
        public string? ProjectIdParam { get; set; }

        /// <summary>
        /// Gets or sets roles that bypass all checks (e.g., "GlobalAdministrator").
        /// </summary>
        public string[]? BypassForRoles { get; set; } = [Roles.GlobalAdministrator];

        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var accessChecker = serviceProvider.GetRequiredService<IAccessChecker>();
            var authScopeProvider = serviceProvider.GetRequiredService<IAuthScopeProvider>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var logger = serviceProvider.GetRequiredService<ILogger<UserContextAuthorizationFilter>>();

            return new UserContextAuthorizationFilter(
                accessChecker,
                authScopeProvider,
                mediator,
                logger,
                RequireContext,
                RequireIncubator,
                RequireProject,
                BusinessIncubatorIdParam,
                ProjectIdParam,
                BypassForRoles);
        }
    }
}