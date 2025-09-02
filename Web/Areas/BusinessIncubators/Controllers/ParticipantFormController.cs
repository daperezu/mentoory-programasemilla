using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Submit;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmission;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectFormStructure;
using LinaSys.Web.Areas.BusinessIncubators.Models.ParticipantForm;
using LinaSys.Web.Controllers;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

/// <summary>
/// Controller for participant form completion.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ParticipantFormController"/> class.
/// </remarks>
/// <param name="mediator">The mediator executor.</param>
/// <param name="repository">The business incubator repository.</param>
[Area("BusinessIncubators")]
[Route("[area]/{businessIncubatorExternalId:guid}/Projects/{projectExternalId:guid}/[controller]")]
public class ParticipantFormController(
    ILogger<ParticipantFormController> logger,
    MediatorExecutor mediator,
    BusinessIncubator.Domain.Repositories.IBusinessIncubatorRepository repository)
    : AuthorizedBaseController(logger, mediator)
{

    /// <summary>
    /// Displays the form wizard for participants.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromQuery] long? formId)
    {
        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        // Verify participant has access to the project
        var project = await repository.GetProjectByExternalIdAsync(projectExternalId);
        if (project is null)
        {
            return NotFound();
        }

        // Check if user has form access through the domain
        var hasAccess = project.HasFormAccess(userId);
        if (!hasAccess)
        {
            TempData["ErrorMessage"] = "No tienes acceso a este proyecto.";
            return RedirectToAction("Index", "Home");
        }

        // For now, default to form ID 1 if not specified
        var selectedFormId = formId ?? 1;

        // Get form submission status
        var submission = await MediatorExecutor.SendOrThrowAsync(new GetFormSubmissionQuery
        {
            ProjectExternalId = projectExternalId,
            ParticipantUserId = userId,
            FormId = selectedFormId
        });

        // Check if user can edit the form
        if (!submission.CanEdit)
        {
            TempData["ErrorMessage"] = submission.Status == "Enviado"
                ? "Este formulario ya ha sido enviado y está pendiente de revisión."
                : "Este formulario ya ha sido procesado y no puede ser editado.";
            return RedirectToAction("Index", "Projects", new { businessIncubatorExternalId });
        }

        var model = new ParticipantFormViewModel
        {
            BusinessIncubatorExternalId = businessIncubatorExternalId,
            ProjectExternalId = projectExternalId,
            ProjectId = submission.ProjectId,
            FormId = submission.FormId,
            SubmissionId = submission.Id,
            Status = submission.Status,
            StatusCode = submission.StatusCode,
            DraftData = submission.DraftData,
            CanSubmit = submission.CanSubmit
        };

        return View(model);
    }

    /// <summary>
    /// Saves the form draft via AJAX.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Route("SaveDraft")]
    public async Task<IActionResult> SaveDraft(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromBody] SaveDraftModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        // Verify user owns this submission
        var ownsSubmission = await VerifySubmissionOwnership(model.ProjectId, model.SubmissionId, userId);
        if (!ownsSubmission)
        {
            return Forbid();
        }

        var command = new SaveDraftCommand
        {
            ProjectId = model.ProjectId,
            FormId = model.FormId,
            ParticipantUserId = userId,
            DraftData = model.DraftData
        };

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al procesar la solicitud."] });
        }

        return Ok(new { success = true, savedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Submits the form for review.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Route("Submit")]
    public async Task<IActionResult> Submit(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromBody] SubmitFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        // Verify user owns this submission
        var ownsSubmission = await VerifySubmissionOwnership(model.ProjectId, model.SubmissionId, userId);
        if (!ownsSubmission)
        {
            return Forbid();
        }

        // Get user agent and IP
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        var command = new SubmitFormCommand
        {
            ProjectId = model.ProjectId,
            SubmissionId = model.SubmissionId,
            ParticipantUserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al procesar la solicitud."] });
        }

        return Ok(new
        {
            success = true,
            message = "Formulario enviado exitosamente. Recibirá una notificación cuando sea revisado.",
            redirectUrl = Url.Action("Index", "Projects", new { businessIncubatorExternalId })
        });
    }

    /// <summary>
    /// Gets the form structure via AJAX.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Route("GetFormStructure")]
    public async Task<IActionResult> GetFormStructure(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromQuery] long formId)
    {
        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        // Verify participant has access to the project
        var project = await repository.GetProjectByExternalIdAsync(projectExternalId);
        if (project is null)
        {
            return NotFound();
        }

        // Check if user has form access through the domain
        var hasAccess = project.HasFormAccess(userId);
        if (!hasAccess)
        {
            return Forbid();
        }

        var query = new GetProjectFormStructureQuery
        {
            ProjectId = project.Id,
            FormId = formId
        };

        var result = await MediatorExecutor.SendOrThrowAsync(query);
        return Ok(result);
    }

    private async Task<bool> VerifySubmissionOwnership(long projectId, long submissionId, string userId)
    {
        var project = await repository.GetProjectWithFormSubmissionsAsync(projectId);
        if (project is null)
        {
            return false;
        }

        var submission = project.GetFormSubmission(submissionId);
        return submission is not null && submission.ParticipantUserId == userId;
    }
}
