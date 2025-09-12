using LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;
using LinaSys.BusinessIncubator.Application.Project.Commands;
using LinaSys.BusinessIncubator.Application.Project.Queries;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetParticipantSubmissions;
using LinaSys.Diagnostics.Application.Form.Queries;
using LinaSys.Orchestration.Application.BusinessIncubator.Commands;
using LinaSys.Shared.Application;
using LinaSys.Web.Areas.BusinessIncubators.Models.Project;
using LinaSys.Web.Areas.BusinessIncubators.Models.Projects;
using LinaSys.Web.Attributes;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

[Area("BusinessIncubators")]
[Route("BusinessIncubators/{businessIncubatorId:guid}/Projects")]
public class ProjectsController(ILogger<ProjectsController> logger, MediatorExecutor mediator, IApplicationUrlService applicationUrlService)
    : AuthorizedBaseController(logger, mediator, applicationUrlService)
{
    [HttpGet("Create")]
    public async Task<IActionResult> Create(Guid businessIncubatorId)
    {
        var viewModel = new CreateProjectViewModel()
        {
            BusinessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId),
        };

        return View(viewModel);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(Guid businessIncubatorId, CreateProjectViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var createResult = await MediatorExecutor.SendAndLogIfFailureAsync(new CreateProjectCommand(
                BusinessIncubatorExternalId: businessIncubatorId,
                Name: viewModel.Name,
                Description: viewModel.Description,
                Key: viewModel.Key));

            if (createResult.IsSuccess)
            {
                this.SetSuccessToast("Proyecto creado");
                return RedirectToAction("Edit", new { businessIncubatorId, projectId = createResult.Value });
            }

            MapErrorsToModelStateAndSetErrorToast<CreateProjectCommand>(createResult);
        }

        viewModel.BusinessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId);

        return View(viewModel);
    }

    [HttpGet("Edit/{projectId:guid}")]
    public async Task<IActionResult> Edit(Guid businessIncubatorId, Guid projectId)
    {
        var businessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId);

        var viewModel = new EditProjectViewModel();

        return View(viewModel);
    }

    [HttpPost("Edit/{projectId:guid}")]
    public async Task<IActionResult> Edit(Guid businessIncubatorId, Guid projectId, EditProjectViewModel viewModel)
    {
        var businessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId);

        return View(viewModel);
    }

    public async Task<IActionResult> Index(Guid businessIncubatorId)
    {
        var viewModel = new ManageProjectsViewModel
        {
            BusinessIncubatorId = businessIncubatorId,
            BusinessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId),
        };

        return View(viewModel);
    }

    [HttpPost("List")]
    public async Task<IActionResult> List(Guid businessIncubatorId, DataTableRequest request)
    {
        var query = new ListProjectsQuery(
            BusinessIncubatorExternalId: businessIncubatorId,
            Start: request.Start,
            Length: request.Length,
            Name: request.ColumnSearches.GetValueOrDefault("name"),
            Description: request.ColumnSearches.GetValueOrDefault("description"),
            Key: request.ColumnSearches.GetValueOrDefault("key"),
            StatusId: int.TryParse(request.ColumnSearches.GetValueOrDefault("status"), out var s) ? s : null,
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection);

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return result.ToJson(request);
    }

    [HttpGet("CopyDiagnosticsForm")]
    [RestoreModelAndState<CopyDiagnosticsFormViewModel>]
    public async Task<IActionResult> CopyDiagnosticsForm(Guid businessIncubatorId, CopyDiagnosticsFormViewModel viewModel)
    {
        viewModel.BusinessIncubatorExternalId = businessIncubatorId;
        viewModel.BusinessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId);

        if (!viewModel.WasRestored)
        {
            ModelState.Clear();
        }

        await PopulateDropdownsAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost("CopyDiagnosticsForm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CopyDiagnosticsFormPost(Guid businessIncubatorId, CopyDiagnosticsFormViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            // Reset project content if requested
            if (viewModel.ResetProjectBeforeCopy)
            {
                var resetCommand = new ResetProjectCommand(
                    BusinessIncubatorExternalId: businessIncubatorId,
                    ProjectExternalId: viewModel.ProjectExternalId,
                    ResetBlocks: true,
                    ResetKnowledgeStructure: true);

                var resetResult = await MediatorExecutor.SendAndLogIfFailureAsync(resetCommand);

                if (!resetResult.IsSuccess)
                {
                    MapErrorsToModelStateAndSetErrorToast<ResetProjectCommand>(resetResult);
                    viewModel.BusinessIncubatorExternalId = businessIncubatorId;
                    viewModel.BusinessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId);
                    await PopulateDropdownsAsync(viewModel);
                    return View(viewModel);
                }
            }

            var command = new CopyDiagnosticsFormToBusinessIncubatorProjectCommand(
                BusinessIncubatorExternalId: businessIncubatorId,
                FormId: viewModel.FormId,
                ProjectExternalId: viewModel.ProjectExternalId);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

            if (result.IsSuccess)
            {
                var message = viewModel.ResetProjectBeforeCopy
                    ? "El proyecto ha sido limpiado y el formulario de diagnóstico ha sido copiado exitosamente."
                    : "El formulario de diagnóstico ha sido copiado exitosamente al proyecto.";
                this.SetSuccessToast(message);
                return RedirectToAction("Index", new { businessIncubatorId });
            }

            MapErrorsToModelStateAndSetErrorToast<CopyDiagnosticsFormToBusinessIncubatorProjectCommand>(result);
        }

        viewModel.BusinessIncubatorExternalId = businessIncubatorId;
        viewModel.BusinessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId);

        await PopulateDropdownsAsync(viewModel);

        return View(viewModel);
    }

    [HttpGet("{projectId:guid}/Invitations")]
    public async Task<IActionResult> Invitations(Guid businessIncubatorId, Guid projectId)
    {
        var projectQuery = new LinaSys.BusinessIncubator.Application.Queries.GetProjectByExternalIdQuery(projectId);
        var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);

        var viewModel = new ProjectInvitationsViewModel
        {
            BusinessIncubatorId = businessIncubatorId,
            ProjectId = projectId,
            BusinessIncubatorName = await GetBusinessIncubatorNameAsync(businessIncubatorId),
            ProjectName = projectResult.Value?.Name ?? "Proyecto",
        };

        return View(viewModel);
    }

    [HttpPost("{projectId:guid}/Invitations/List")]
    public async Task<IActionResult> ListInvitations(Guid businessIncubatorId, Guid projectId, DataTableRequest request)
    {
        var query = new ListProjectInvitationsQuery(
            ProjectExternalId: projectId,
            Start: request.Start,
            Length: request.Length,
            Search: request.GlobalSearch,
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection,
            Email: request.ColumnSearches.GetValueOrDefault("email"),
            Status: request.ColumnSearches.GetValueOrDefault("status"));

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        var filteredResult = FilteredQueryResult.From(result.Data, result.TotalRecords, result.FilteredRecords);
        return filteredResult.ToJson(request);
    }

    [HttpPost("{projectId:guid}/Invitations/{invitationId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessInvitation(Guid businessIncubatorId, Guid projectId, Guid invitationId, [FromForm] string action)
    {
        try
        {
            // First, we need to get the invitation to find the token
            var getInvitationQuery = new GetProjectInvitationByIdQuery(projectId, invitationId);
            var invitationResult = await MediatorExecutor.SendOrThrowAsync(getInvitationQuery);

            if (invitationResult is null)
            {
                this.SetErrorToast("La invitación no fue encontrada.");
                return RedirectToAction(nameof(Invitations), new { businessIncubatorId, projectId });
            }

            var invitationAction = action.ToLowerInvariant() switch
            {
                "revoke" => InvitationAction.Revoke,
                "resend" => InvitationAction.Accept, // Temporary - we need a resend action
                _ => throw new ArgumentException("Acción de invitación no válida"),
            };

            // For resend, we'll just show a message since we don't have resend functionality yet
            if (action.ToLowerInvariant() == "resend")
            {
                this.SetErrorToast("La funcionalidad de reenvío aún no está implementada.");
                return RedirectToAction(nameof(Invitations), new { businessIncubatorId, projectId });
            }

            var command = new ProcessProjectInvitationCommand(
                invitationResult.InvitationToken,
                invitationAction);

            var result = await MediatorExecutor.SendOrThrowAsync(command);

            if (result.Success)
            {
                this.SetSuccessToast(result.Message ?? "Operación completada exitosamente.");
            }
            else
            {
                this.SetErrorToast(result.Message ?? "Error al procesar la invitación.");
            }
        }
        catch (Exception ex)
        {
            this.SetErrorToast($"Error procesando la invitación: {ex.Message}");
        }

        return RedirectToAction("Invitations", new { businessIncubatorId, projectId });
    }

    [HttpGet("{projectId:guid}/Dashboard")]
    public async Task<IActionResult> ParticipantDashboard(
        Guid businessIncubatorId,
        Guid projectId)
    {
        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        // Get project and verify access
        var projectQuery = new LinaSys.BusinessIncubator.Application.Queries.GetProjectByExternalIdQuery(
            projectId,
            CheckAccessForUserId: userId);
        var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);

        if (!projectResult.IsSuccess || projectResult.Value is null)
        {
            return NotFound();
        }

        // Check if user has access through UserProjectAccess (Auth domain)
        if (projectResult.Value.HasAccess == false)
        {
            TempData["ErrorMessage"] = "No tienes acceso a este proyecto.";
            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }

        // Get participant's form submissions
        var submissionsQuery = new GetParticipantSubmissionsQuery
        {
            ProjectExternalId = projectId,
            ParticipantUserId = userId
        };

        var submissionsResult = await MediatorExecutor.SendOrThrowAsync(submissionsQuery);

        var viewModel = new ParticipantDashboardViewModel
        {
            BusinessIncubatorExternalId = businessIncubatorId,
            ProjectId = projectId,
            ProjectName = projectResult.Value.Name,
            FormSubmissions = submissionsResult
        };

        return View(viewModel);
    }

    private async Task PopulateDropdownsAsync(CopyDiagnosticsFormViewModel viewModel)
    {
        // Get available forms
        var formsQuery = new ListFormsQuery(
            Start: 0,
            Length: 100,
            Name: null,
            OrderByColumn: "Name",
            OrderDirection: "asc");

        var formsResult = await MediatorExecutor.SendOrThrowAsync(formsQuery);

        viewModel.FormOptions = formsResult.Data
            .Select(form => new SelectListItem
            {
                Value = form.Id.ToString(),
                Text = form.Name,
                Selected = form.Id == viewModel.FormId,
            })
            .ToList();

        // Add default option
        viewModel.FormOptions.Insert(0, new SelectListItem
        {
            Value = string.Empty,
            Text = "-- Seleccione un formulario --",
            Selected = viewModel.FormId == 0,
        });

        // Get available projects for the business incubator
        var projectsQuery = new ListProjectsQuery(
            BusinessIncubatorExternalId: viewModel.BusinessIncubatorExternalId,
            Start: 0,
            Length: 100,
            Name: null,
            Description: null,
            Key: null,
            StatusId: 1, // Only active projects
            OrderByColumn: "Name",
            OrderDirection: "asc");

        var projectsResult = await MediatorExecutor.SendOrThrowAsync(projectsQuery);

        viewModel.ProjectOptions = projectsResult.Data
            .Select(project => new SelectListItem
            {
                Value = project.ExternalId.ToString(),
                Text = $"{project.Name} ({project.Key})",
                Selected = project.ExternalId == viewModel.ProjectExternalId,
            })
            .ToList();

        // Add default option
        viewModel.ProjectOptions.Insert(0, new SelectListItem
        {
            Value = string.Empty,
            Text = "-- Seleccione un proyecto --",
            Selected = viewModel.ProjectExternalId == Guid.Empty,
        });
    }

    private async Task<string> GetBusinessIncubatorNameAsync(Guid businessIncubatorId)
    {
        var businessIncubatorDetails = await MediatorExecutor.SendOrThrowAsync(new GetBusinessIncubatorDetailsQuery(businessIncubatorId));
        return businessIncubatorDetails.Name;
    }
}
