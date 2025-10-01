using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.GetOrCreateFormSubmission;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Areas.Participant.Models;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Participant.Controllers;

[Area("Participant")]
[Authorize(Roles = Roles.Starter)]
public class DashboardController(
    ILogger<DashboardController> logger,
    MediatorExecutor mediator,
    IApplicationUrlService applicationUrlService) : AuthorizedBaseController(logger, mediator, applicationUrlService)
{
    private readonly IApplicationUrlService _applicationUrlService = applicationUrlService;
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Require project context for Starters
        var context = DemandCurrentUserContext(requireProject: true,
            errorMessage: "Debe seleccionar un proyecto para ver el panel de control");

        var userId = CurrentUserId;
        var projectId = context.ProjectId!.Value;

        // Get single project details instead of all projects
        var projectQuery = new GetProjectDetailsQuery(projectId);
        var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);

        // Get available forms for this specific project
        var availableFormsQuery = new GetAvailableFormsQuery(userId, projectId);
        var availableFormsResult = await MediatorExecutor.SendAndLogIfFailureAsync(availableFormsQuery);

        // Get project-specific activities
        var activitiesQuery = new GetProjectActivitiesQuery(userId, projectId, 10);
        var activitiesResult = await MediatorExecutor.SendAndLogIfFailureAsync(activitiesQuery);

        var viewModel = new LinaSys.Web.Areas.Participant.Models.ProjectDashboardViewModel
        {
            UserName = User.Identity?.Name ?? "Usuario",
            Project = MapProjectToViewModel(projectResult),
            AvailableForms = MapAvailableFormsToViewModel(
                availableFormsResult,
                projectResult.Value?.IncubatorExternalId,
                projectResult.Value?.ExternalId),
            RecentActivities = MapActivitiesToViewModel(activitiesResult),
            SelectedProjectName = projectResult.Value?.Name ?? "Proyecto"
        };

        return View(viewModel);
    }

    [HttpGet("Forms/Start")]
    public async Task<IActionResult> StartForm(BusinessIncubator.Domain.Enums.QuestionPhase phase)
    {
        var context = DemandCurrentUserContext(requireProject: true);

        // Get project details including business incubator external ID
        var projectDetailsQuery = new GetProjectDetailsQuery(context.ProjectId!.Value);
        var projectDetailsResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectDetailsQuery);

        if (!projectDetailsResult.IsSuccess || projectDetailsResult.Value == null)
        {
            this.SetErrorToast("Proyecto no encontrado");
            return RedirectToAction("Index");
        }

        // Get project external ID for the command
        var projectQuery = new GetProjectByIdQuery(context.ProjectId!.Value);
        var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);

        if (!projectResult.IsSuccess || projectResult.Value == null)
        {
            this.SetErrorToast("Proyecto no encontrado");
            return RedirectToAction("Index");
        }

        // Trigger lazy form creation
        var command = new GetOrCreateFormSubmissionCommand(
            projectResult.Value.ExternalId,
            CurrentUserId,
            phase);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess && projectDetailsResult.Value.IncubatorExternalId.HasValue)
        {
            // Use ApplicationUrlService to generate the correct URL
            var url = _applicationUrlService.GetParticipantFormUrl(
                projectDetailsResult.Value.IncubatorExternalId.Value,
                projectResult.Value.ExternalId);

            return Redirect(url);
        }

        this.SetErrorToast("No se pudo iniciar el formulario");
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult MyProjects()
    {
        // TODO: Implement active projects view
        return View();
    }

    [HttpGet]
    public IActionResult OpenConvocations()
    {
        // TODO: Implement available opportunities view
        return View();
    }

    [HttpGet]
    public IActionResult PendingForms()
    {
        // TODO: Implement forms requiring attention view
        return View();
    }

    private static string GetFormTitle(BusinessIncubator.Domain.Enums.QuestionPhase phase)
    {
        return phase switch
        {
            BusinessIncubator.Domain.Enums.QuestionPhase.Start => "Formulario de Inicio",
            BusinessIncubator.Domain.Enums.QuestionPhase.Final => "Formulario Final",
            BusinessIncubator.Domain.Enums.QuestionPhase.Both => "Formulario Completo",
            _ => "Formulario"
        };
    }

    private static string GetPhaseDisplayName(BusinessIncubator.Domain.Enums.QuestionPhase phase)
    {
        return phase switch
        {
            BusinessIncubator.Domain.Enums.QuestionPhase.Start => "Inicio",
            BusinessIncubator.Domain.Enums.QuestionPhase.Final => "Final",
            BusinessIncubator.Domain.Enums.QuestionPhase.Both => "Completo",
            _ => phase.ToString()
        };
    }

    private static string GetActivityIcon(string activityType)
    {
        return activityType.ToLowerInvariant() switch
        {
            "login" => "bi-box-arrow-in-right",
            "logout" => "bi-box-arrow-right",
            "project_joined" => "bi-folder-plus",
            "form_submitted" => "bi-file-earmark-check",
            "form_approved" => "bi-check-circle",
            "form_rejected" => "bi-x-circle",
            "profile_updated" => "bi-person-gear",
            "password_changed" => "bi-key",
            "email_verified" => "bi-envelope-check",
            "role_assigned" => "bi-shield-check",
            "invitation_sent" => "bi-envelope-plus",
            "invitation_accepted" => "bi-person-check",
            _ => "bi-info-circle"
        };
    }

    private static string GetActivityColor(string activityType)
    {
        return activityType.ToLowerInvariant() switch
        {
            "login" or "logout" => "info",
            "project_joined" or "invitation_accepted" => "success",
            "form_submitted" => "primary",
            "form_approved" => "success",
            "form_rejected" => "danger",
            "profile_updated" or "password_changed" => "warning",
            "email_verified" or "role_assigned" => "success",
            "invitation_sent" => "info",
            _ => "secondary"
        };
    }

    private ProjectDetailsViewModel MapProjectToViewModel(Result<ProjectDetailsDto> result)
    {
        if (!result.IsSuccess || result.Value == null)
        {
            return new ProjectDetailsViewModel();
        }

        var project = result.Value;
        return new ProjectDetailsViewModel
        {
            ProjectId = project.ProjectId,
            ExternalId = project.ExternalId,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            CurrentStage = project.CurrentStage,
            StageEndDate = project.StageEndDate,
            Progress = project.Progress,
            IncubatorName = project.IncubatorName,
            IncubatorExternalId = project.IncubatorExternalId,
            MentorName = project.MentorName,
            StartDate = project.StartDate,
            EndDate = project.EndDate
        };
    }

    private List<AvailableFormViewModel> MapAvailableFormsToViewModel(
        Result<List<AvailableFormDto>> result,
        Guid? businessIncubatorExternalId,
        Guid? projectExternalId)
    {
        if (!result.IsSuccess || result.Value == null)
        {
            return new List<AvailableFormViewModel>();
        }

        return result.Value.Select(f => new AvailableFormViewModel
        {
            ExistingFormId = f.ExistingFormId,
            FormName = GetFormTitle(f.Phase),
            Phase = f.Phase,
            StageName = f.StageName,
            DueDate = f.DueDate,
            IsCreated = f.IsCreated,
            Status = f.Status ?? BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Draft,
            CompletionPercentage = f.CompletionPercentage,
            PendingFeedbackCount = f.PendingFeedbackCount,
            ActionUrl = GetFormActionUrl(f, businessIncubatorExternalId, projectExternalId),
            ActionText = f.IsCreated
                ? (f.Status == BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Draft
                    ? "Continuar"
                    : f.PendingFeedbackCount > 0
                        ? "Responder Retroalimentación"
                        : "Ver Formulario")
                : "Iniciar Formulario",
            ActionClass = f.IsCreated
                ? (f.Status == BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Draft
                    ? "btn-warning"
                    : f.PendingFeedbackCount > 0
                        ? "btn-danger"
                        : "btn-info")
                : "btn-primary"
        }).ToList();
    }

    private string GetFormActionUrl(AvailableFormDto form, Guid? businessIncubatorExternalId, Guid? projectExternalId)
    {
        // For submitted forms, always show them in read-only mode
        if (form.Status == BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Submitted ||
            form.Status == BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Approved ||
            form.Status == BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Rejected)
        {
            // If we have all required IDs, generate the ParticipantForm URL with read-only parameter
            if (businessIncubatorExternalId.HasValue && projectExternalId.HasValue)
            {
                var url = _applicationUrlService.GetParticipantFormUrl(
                    businessIncubatorExternalId.Value,
                    projectExternalId.Value);

                // Add read-only parameter for submitted/approved forms ONLY if there's no pending feedback
                // Forms with pending feedback need to allow responses even if submitted
                if ((form.Status == BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Submitted ||
                     form.Status == BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Approved) &&
                    form.PendingFeedbackCount == 0)
                {
                    url = url.Contains('?') ? $"{url}&readOnly=true" : $"{url}?readOnly=true";
                }

                return url;
            }
        }

        // If form is already created and we have all required IDs, use the ParticipantForm URL
        if (form.IsCreated && businessIncubatorExternalId.HasValue && projectExternalId.HasValue)
        {
            return _applicationUrlService.GetParticipantFormUrl(
                businessIncubatorExternalId.Value,
                projectExternalId.Value);
        }

        // If form is created but missing IDs (shouldn't happen, but handle gracefully)
        if (form.IsCreated)
        {
            // Log warning about missing data
            logger.LogWarning(
                "Form {FormId} is marked as created but missing required IDs. Status: {Status}, BusinessIncubatorId: {BusinessIncubatorId}, ProjectId: {ProjectId}",
                form.ExistingFormId,
                form.Status,
                businessIncubatorExternalId,
                projectExternalId);

            // Return a placeholder URL or the dashboard
            return Url.Action("Index", "Dashboard", new { area = "Participant" }) ?? "#";
        }

        // For forms that haven't been created yet, use the StartForm action
        return Url.Action("StartForm", "Dashboard", new { area = "Participant", phase = form.Phase }) ?? "#";
    }

    private List<LinaSys.Web.Areas.Participant.Models.ProjectActivityViewModel> MapActivitiesToViewModel(Result<List<ProjectActivityDto>> result)
    {
        if (!result.IsSuccess || result.Value == null)
        {
            return new List<LinaSys.Web.Areas.Participant.Models.ProjectActivityViewModel>();
        }

        return result.Value.Select(a => new LinaSys.Web.Areas.Participant.Models.ProjectActivityViewModel
        {
            Category = a.Category,
            Description = a.Description,
            Timestamp = a.Timestamp,
            Icon = a.Icon,
            IconColor = a.IconColor
        }).ToList();
    }

    private string GetActivityDescription(string activityType, string? entityType)
    {
        return activityType.ToLowerInvariant() switch
        {
            "login" => "Has iniciado sesión",
            "project_joined" => $"Te uniste al proyecto {entityType ?? string.Empty}",
            "form_submitted" => "Formulario enviado para revisión",
            "form_approved" => "Tu formulario ha sido aprobado",
            "form_rejected" => "Tu formulario requiere cambios",
            _ => activityType
        };
    }

    private string GetActivityTypeForDisplay(string activityType)
    {
        return activityType.ToLowerInvariant() switch
        {
            "login" or "logout" => "info",
            "project_joined" or "form_approved" => "success",
            "form_submitted" => "primary",
            "form_rejected" => "warning",
            _ => "secondary"
        };
    }
}
