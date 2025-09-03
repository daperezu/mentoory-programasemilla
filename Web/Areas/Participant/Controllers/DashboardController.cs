using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Core.Application.Activities.Queries;
using LinaSys.Shared.Application;
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
    MediatorExecutor mediator) : AuthorizedBaseController(logger, mediator)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Get current user's context
        var userId = CurrentUserId;
        var userContext = CurrentUserContext;

        // Get user's projects from BusinessIncubator domain
        var projectsQuery = new GetParticipantProjectsQuery(userId);
        var projectsResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectsQuery);

        // Get pending forms from BusinessIncubator domain
        var pendingFormsQuery = new GetPendingFormsQuery(userId);
        var pendingFormsResult = await MediatorExecutor.SendAndLogIfFailureAsync(pendingFormsQuery);

        // Get recent activities from audit system
        var activitiesQuery = new GetUserActivitiesQuery(userId, 10);
        var activitiesResult = await MediatorExecutor.SendAndLogIfFailureAsync(activitiesQuery);

        // TODO: Get open convocations - needs separate implementation
        var viewModel = new DashboardViewModel
        {
            UserName = User.Identity?.Name ?? "Usuario",
            Projects = MapProjectsToViewModel(projectsResult),
            PendingForms = MapPendingFormsToViewModel(pendingFormsResult),
            RecentActivities = MapActivitiesToViewModel(activitiesResult),
            OpenConvocations = new List<ConvocationViewModel>() // TODO: Implement when convocations are available
        };

        return View(viewModel);
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

    private List<ProjectCardViewModel> MapProjectsToViewModel(Result<List<ParticipantProjectDto>> result)
    {
        if (!result.IsSuccess || result.Value == null)
        {
            return new List<ProjectCardViewModel>();
        }

        return result.Value.Select(p => new ProjectCardViewModel
        {
            ProjectId = p.Id,
            ProjectName = p.Name,
            Description = string.Empty, // Not available in DTO
            CurrentStage = p.CurrentStageName ?? "Sin etapa",
            Progress = (int)p.ProgressPercentage,
            IncubatorName = p.IncubatorName,
            Status = p.Status == "active" ? "Activo" : "Inactivo",
            StatusColor = p.Status == "active" ? "success" : "secondary",
            StartDate = p.JoinedAt,
            EndDate = null,
            IsActive = p.Status == "active",
            MentorName = string.Empty // TODO: Get mentor from project assignments
        }).ToList();
    }

    private List<PendingFormViewModel> MapPendingFormsToViewModel(Result<List<PendingFormDto>> result)
    {
        if (!result.IsSuccess || result.Value == null)
        {
            return new List<PendingFormViewModel>();
        }

        return result.Value.Select(f => new PendingFormViewModel
        {
            FormId = f.Id,
            FormName = GetFormTitle(f.Phase),
            ProjectName = f.ProjectName,
            DueDate = f.DueDate ?? DateTime.UtcNow.AddDays(7), // Default to 7 days if no due date
            FormType = GetPhaseDisplayName(f.Phase),
            FormUrl = $"/ProjectFormSubmission/Edit/{f.ExternalId}" // TODO: Verify actual URL pattern
        }).ToList();
    }

    private List<ActivityViewModel> MapActivitiesToViewModel(Result<List<UserActivityDto>> result)
    {
        if (!result.IsSuccess || result.Value == null)
        {
            return new List<ActivityViewModel>();
        }

        return result.Value.Select(a => new ActivityViewModel
        {
            Timestamp = a.CreatedDate,
            Type = a.ActivityType,
            Category = GetActivityTypeForDisplay(a.ActivityType),
            Description = GetActivityDescription(a.ActivityType, a.EntityType),
            Icon = GetActivityIcon(a.ActivityType),
            IconColor = GetActivityColor(a.ActivityType)
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
