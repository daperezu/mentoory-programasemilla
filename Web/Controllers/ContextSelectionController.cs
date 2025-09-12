using LinaSys.Auth.Application.Commands.Context;
using LinaSys.Auth.Application.Queries.Context;
using LinaSys.Orchestration.Application.Context;
using LinaSys.Shared.Application.Services;
using LinaSys.Web.Models.Context;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Controllers;

public class ContextSelectionController(
    ILogger<ContextSelectionController> logger,
    MediatorExecutor mediatorExecutor,
    IApplicationUrlService applicationUrlService)
    : AuthorizedBaseController(logger, mediatorExecutor, applicationUrlService)
{
    public const string IndexUrl = "/ContextSelection/Index";

    private readonly IApplicationUrlService _applicationUrlService = applicationUrlService;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("ContextSelection.Index called for user: {UserId}", CurrentUserId);

        // Get user roles
        var userRoles = CurrentUserRoles;
        // Check for single role auto-selection
        string? selectedRole = null;
        if (userRoles.Count == 1)
        {
            selectedRole = userRoles[0];
            logger.LogInformation("User {UserId} has single role: {Role}", CurrentUserId, selectedRole);
        }

        // Initialize variables for auto-selection logic
        List<UserIncubatorViewModel>? incubators = null;
        List<UserProjectDto>? projects = null;
        long? selectedIncubatorId = null;
        long? selectedProjectId = null;

        // If we have a role (single or selected), check incubators
        if (!string.IsNullOrEmpty(selectedRole))
        {
            // Check if role needs incubator/project
            var isGlobalAdmin = selectedRole == Shared.Domain.Constants.Roles.GlobalAdministrator;
            var isAdmin = selectedRole == Shared.Domain.Constants.Roles.Administrator;

            if (isGlobalAdmin)
            {
                // Global Admin can optionally select incubator/project
                // Don't auto-redirect, let them choose if they want
                logger.LogInformation("User {UserId} is Global Admin, showing optional context selection", CurrentUserId);

                // Load all incubators they have access to (optional selection)
                var globalIncubatorsQuery = new GetEnrichedUserActiveIncubatorsQuery(CurrentUserId, selectedRole);
                var globalIncubatorsAssigned = await MediatorExecutor.SendOrThrowAsync(globalIncubatorsQuery);
                incubators = globalIncubatorsAssigned.Select(i => new UserIncubatorViewModel
                {
                    IncubatorId = i.IncubatorId,
                    Name = i.Name
                }).ToList();
            }
            else
            {
                // Load incubators for the role
                var incubatorsQuery = new GetEnrichedUserActiveIncubatorsQuery(CurrentUserId, selectedRole);
                var incubatorsAssigned = await MediatorExecutor.SendOrThrowAsync(incubatorsQuery);
                incubators = incubatorsAssigned.Select(i => new UserIncubatorViewModel
                {
                    IncubatorId = i.IncubatorId,
                    Name = i.Name
                }).ToList();
            }

            // Check for single incubator auto-selection
            if (incubators.Count == 1)
            {
                selectedIncubatorId = incubators[0].IncubatorId;
                logger.LogInformation("User {UserId} has single incubator: {IncubatorId}", CurrentUserId, selectedIncubatorId);

                if (isAdmin)
                {
                    // Administrator only needs role + incubator, auto-save and redirect
                    logger.LogInformation("User {UserId} is Administrator with single incubator, auto-selecting context", CurrentUserId);
                    var adminCommand = new SelectUserContextCommand(
                        CurrentUserId,
                        selectedRole,
                        selectedIncubatorId,
                        null);
                    var adminResult = await MediatorExecutor.SendOrThrowAsync(adminCommand);
                    return RedirectToAction("RedirectToDashboard", "AuthRedirect");
                }

                // For other roles, check projects
                var projectsQuery = new GetEnrichedUserProjectsQuery(CurrentUserId, selectedRole, selectedIncubatorId.Value);
                var projectsResult = await MediatorExecutor.SendOrThrowAsync(projectsQuery);
                projects = projectsResult.Select(p => new UserProjectDto
                {
                    ProjectId = p.ProjectId,
                    Name = p.Name,
                    UserRole = p.UserRole
                }).ToList();

                // Check for single project auto-selection
                if (projects.Count == 1)
                {
                    selectedProjectId = projects[0].ProjectId;
                    logger.LogInformation("User {UserId} has single project: {ProjectId}, auto-selecting full context", CurrentUserId, selectedProjectId);
                    // Auto-save and redirect
                    var fullCommand = new SelectUserContextCommand(
                        CurrentUserId,
                        selectedRole,
                        selectedIncubatorId,
                        selectedProjectId);
                    var fullResult = await MediatorExecutor.SendOrThrowAsync(fullCommand);
                    return RedirectToAction("RedirectToDashboard", "AuthRedirect");
                }
            }
        }

        // If we reach here, user needs to make selections - prepare the view model
        var model = new ContextSelectionViewModel
        {
            Roles = userRoles,
            SelectedRole = selectedRole,
            CurrentRole = selectedRole,
            CurrentIncubatorId = selectedIncubatorId,
            CurrentProjectId = selectedProjectId,
            Incubators = incubators ?? [],
            Projects = projects ?? [],
            SelectedIncubatorId = selectedIncubatorId,
            SelectedProjectId = selectedProjectId,
            LogoutUrl = _applicationUrlService.GetLogoutUrl()
        };

        logger.LogInformation("User {UserId} needs to select context, showing selection page", CurrentUserId);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SelectContext([FromBody] SelectContextRequest request)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Datos inválidos" });
        }

        // Validate based on role
        var isGlobalAdmin = request.Role == Shared.Domain.Constants.Roles.GlobalAdministrator;
        var isAdmin = request.Role == Shared.Domain.Constants.Roles.Administrator;

        // Global Admin can save with just role
        if (!isGlobalAdmin && !isAdmin && !request.IncubatorId.HasValue)
        {
            return Json(new { success = false, message = "Debe seleccionar una incubadora" });
        }

        // Non-admin roles need both incubator and project (except Global Admin)
        if (!isGlobalAdmin && !isAdmin && !request.ProjectId.HasValue)
        {
            return Json(new { success = false, message = "Debe seleccionar un proyecto" });
        }

        var command = new SelectUserContextCommand(
            CurrentUserId,
            request.Role,
            request.IncubatorId,
            request.ProjectId);

        var result = await MediatorExecutor.SendOrThrowAsync(command);

        // Return with redirect URL
        return Json(new
        {
            success = true,
            redirectUrl = Url.Action("RedirectToDashboard", "AuthRedirect"),
            context = new
            {
                role = result.Role,
                incubatorId = result.IncubatorId,
                projectId = result.ProjectId,
                isGlobalAdmin = result.IsGlobalAdministrator
            }
        });
    }

    [HttpPost]
    public async Task<IActionResult> LoadContext()
    {
        var command = new GetLastUserContextCommand(CurrentUserId);
        var result = await MediatorExecutor.SendOrThrowAsync(command);

        if (result is null)
        {
            return Json(new { success = false, message = "No se encontró contexto guardado" });
        }

        return Json(new
        {
            success = true,
            context = new
            {
                role = result.Role,
                incubatorId = result.IncubatorId,
                projectId = result.ProjectId,
                isGlobalAdmin = result.IsGlobalAdministrator,
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetIncubators(string role)
    {
        if (string.IsNullOrEmpty(role))
        {
            return Json(new { success = false, incubators = new List<object>() });
        }

        // Use orchestration query to get enriched data
        var query = new GetEnrichedUserActiveIncubatorsQuery(CurrentUserId, role);
        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return Json(new
        {
            success = true,
            incubators = result.Select(i => new
            {
                id = i.IncubatorId,
                name = i.Name
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects(string role, long incubatorId)
    {
        if (string.IsNullOrEmpty(role) || incubatorId <= 0)
        {
            return Json(new { success = false, projects = new List<object>() });
        }

        // Use orchestration query to get enriched data
        var query = new GetEnrichedUserProjectsQuery(CurrentUserId, role, incubatorId);
        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return Json(new
        {
            success = true,
            projects = result.Select(p => new
            {
                id = p.ProjectId,
                name = p.Name,
                role = p.UserRole
            })
        });
    }
}
