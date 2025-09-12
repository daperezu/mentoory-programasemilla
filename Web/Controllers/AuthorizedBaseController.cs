using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;
using LinaSys.Auth.Application.Queries.Context;
using LinaSys.Orchestration.Application.UserContext.Commands;
using LinaSys.Shared.Application;
using LinaSys.Web.Attributes;
using LinaSys.Web.Extensions;
using LinaSys.Web.Filters;
using LinaSys.Shared.Application.Services;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Controllers;

[Authorize]
[UserContextAuthorizationFilter.UserContextAuthorization]
public abstract class AuthorizedBaseController(ILogger logger, MediatorExecutor mediatorExecutor, IApplicationUrlService applicationUrlService) : Controller
{
    private EnrichedUserContextDto? _currentUserContext;
    private string? _currentUserId;
    private List<string>? _currentUserRoles;
    private bool _isGlobalAdministrator = false;
    private UserRolesDto? _userRolesDto;

    /// <summary>
    /// Gets the current user context.
    /// </summary>
    /// <remarks>
    /// Lazily loads the enriched context once per controller instance using GetEnrichedUserContextCommand via MediatorExecutor.
    /// Falls back to null when unavailable or when the executor is not provided.
    /// </remarks>
    protected EnrichedUserContextDto? CurrentUserContext
    {
        get
        {
            if (_currentUserContext is not null)
            {
                return _currentUserContext;
            }

            if (!IsAuthenticated)
            {
                return null;
            }

            var result = MediatorExecutor
                .SendOrThrowAsync(new GetEnrichedUserContextCommand(CurrentUserId))
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            _currentUserContext = result;

            return _currentUserContext;
        }
    }

    /// <summary>
    /// Gets the current user ID as a string (cached property).
    /// </summary>
    protected string CurrentUserId => _currentUserId ??= User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    protected List<string> CurrentUserRoles
    {
        get
        {
            EnsureUserRoles();
            return _currentUserRoles ?? [];
        }
    }

    protected bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserId);

    protected bool CurrentUserIsGlobalAdministrator
    {
        get
        {
            EnsureUserRoles();
            return _isGlobalAdministrator;
        }
    }

    protected MediatorExecutor MediatorExecutor { get; } = mediatorExecutor;
    protected IApplicationUrlService ApplicationUrlService { get; } = applicationUrlService;

    protected void HandleInvalidCurrentUserContext()
    {
        if (!IsAuthenticated)
        {
            Response.Redirect(ApplicationUrlService.GetLoginUrl());
            return;
        }

        Response.Redirect(ContextSelectionController.IndexUrl);
    }

    protected void HandleUserNotAuthenticated()
    {
        if (!IsAuthenticated)
        {
            Response.Redirect(ApplicationUrlService.GetLoginUrl());
        }
    }

    protected void MapErrorsToModelStateAndSetErrorToast<T>(Result result)
    {
        if (result.ErrorMessages is not null && result.ErrorMessages.Length > 0)
        {
            var mapper = ResultContextToViewModelMapper.GetMap<T>();

            foreach ((string Context, string Message) resultErrorMessage in result.ErrorMessages)
            {
                ModelState.AddModelError(mapper.GetValueOrDefault(resultErrorMessage.Context, resultErrorMessage.Context), resultErrorMessage.Message);
            }
        }

        var errorCode = result.ErrorCode ?? ResultErrorCodes.Unknown;
        var errorMessage = Resources.ErrorMessages.ResourceManager.GetString(errorCode.ToString());

        this.SetErrorToast(errorMessage ?? "Ocurrió un error: " + errorCode);
    }

    protected IActionResult RedirectToActionWithModel(string? actionName, object? routeValues, object model)
    {
        SaveModelStateToTempData();
        TempData[RestoreModelAndState.TempModelKey] = JsonSerializer.Serialize(model);
        return RedirectToAction(actionName, routeValues);
    }

    protected bool TryGetCurrentUserContext([NotNullWhen(true)] out EnrichedUserContextDto? userContext)
    {
        userContext = CurrentUserContext;
        // Check if context is complete (has role and either is global admin or has incubator/project)
        return userContext != null &&
               !string.IsNullOrEmpty(userContext.Role) &&
               (userContext.IsGlobalAdministrator ||
                userContext is { IncubatorId: not null, ProjectId: not null });
    }

    /// <summary>
    /// Demands that the current user has a valid context with optional project/incubator requirements.
    /// Redirects to login or context selection if requirements are not met.
    /// </summary>
    /// <param name="requireProject">If true, requires a project to be selected (implies incubator requirement).</param>
    /// <param name="requireIncubator">If true, requires an incubator to be selected.</param>
    /// <param name="errorMessage">Optional custom error message to display.</param>
    /// <returns>The current user context if all requirements are met.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated.</exception>
    /// <exception cref="InvalidOperationException">Thrown when context requirements are not met.</exception>
    protected EnrichedUserContextDto DemandCurrentUserContext(
        bool requireProject = false,
        bool requireIncubator = false,
        string? errorMessage = null)
    {
        // Check authentication first
        if (!IsAuthenticated)
        {
            this.SetErrorToast("Debe iniciar sesión para acceder a esta funcionalidad.");
            Response.Redirect(ApplicationUrlService.GetLoginUrl());
            throw new UnauthorizedAccessException("User must be authenticated.");
        }

        var context = CurrentUserContext;

        // Check if context exists and has role
        if (context == null || string.IsNullOrEmpty(context.Role))
        {
            this.SetErrorToast(errorMessage ?? "Debe seleccionar un contexto para continuar.");
            Response.Redirect(ContextSelectionController.IndexUrl);
            throw new InvalidOperationException("User context is required but not available.");
        }

        // If project is required, incubator is implicitly required
        if (requireProject)
        {
            if (context!.ProjectId == null || context.IncubatorId == null)
            {
                this.SetErrorToast(errorMessage ?? "Debe seleccionar un proyecto para acceder a esta funcionalidad.");
                Response.Redirect(ContextSelectionController.IndexUrl);
                throw new InvalidOperationException("Project is required but not selected.");
            }
        }

        // Only check incubator if explicitly required (without project)
        else if (requireIncubator && context!.IncubatorId == null)
        {
            this.SetErrorToast(errorMessage ?? "Debe seleccionar una incubadora para acceder a esta funcionalidad.");
            Response.Redirect(ContextSelectionController.IndexUrl);
            throw new InvalidOperationException("Incubator is required but not selected.");
        }

        return context;
    }

    /// <summary>
    /// Tries to get the current user ID.
    /// </summary>
    /// <param name="userId">The user ID if authenticated.</param>
    /// <returns>True if the user is authenticated; otherwise, false.</returns>
    protected bool TryGetCurrentUserId([NotNullWhen(true)] out string? userId)
    {
        userId = CurrentUserId;
        return !string.IsNullOrEmpty(userId);
    }

    private void EnsureUserRoles()
    {
        if (_userRolesDto is not null)
        {
            return;
        }

        var query = new GetUserRolesQuery(CurrentUserId);
        _userRolesDto = MediatorExecutor
            .SendOrThrowAsync(query)
            .GetAwaiter()
            .GetResult();

        logger.LogInformation("User roles loaded. Roles count: {RoleCount}", _userRolesDto.Roles.Count);

        _currentUserRoles = _userRolesDto.Roles;

        _isGlobalAdministrator = _userRolesDto.IsGlobalAdministrator;
    }

    private void SaveModelStateToTempData()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value is not null && e.Value.Errors.Any())
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage).ToList());

            TempData[RestoreModelAndState.TempModelStateKey] = JsonSerializer.Serialize(errors);
        }
    }
}
