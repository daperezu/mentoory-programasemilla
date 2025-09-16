using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Submit;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmission;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectFormStructure;
using LinaSys.BusinessIncubator.Application.Reviews.Commands.ReplyToFeedback;
using LinaSys.BusinessIncubator.Application.Reviews.Queries.GetFeedbackForSubmission;
using LinaSys.BusinessIncubator.Application.Reviews.Queries.GetPendingFeedbackCount;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Areas.BusinessIncubators.Models.ParticipantForm;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

/// <summary>
/// Controller for participant form completion.
/// </summary>
[Area("BusinessIncubators")]
[Route("[area]/{businessIncubatorExternalId:guid}/Projects/{projectExternalId:guid}/[controller]")]
public class ParticipantFormController(
    ILogger<ParticipantFormController> logger,
    MediatorExecutor mediator,
    IApplicationUrlService applicationUrlService)
    : AuthorizedBaseController(logger, mediator, applicationUrlService)
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
        [FromQuery] string? onBehalfOfUserId = null)
    {
        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        // Determine if this is an on-behalf scenario
        var isOnBehalf = !string.IsNullOrEmpty(onBehalfOfUserId);
        var participantUserId = isOnBehalf ? onBehalfOfUserId : userId;

        // If on-behalf mode, verify the current user is a coordinator/admin
        if (isOnBehalf)
        {
            // Check if user has coordinator access
            var coordinatorQuery = new LinaSys.BusinessIncubator.Application.Queries.IsUserProjectCoordinatorQuery(
                projectExternalId, userId);
            var coordinatorResult = await MediatorExecutor.SendAndLogIfFailureAsync(coordinatorQuery);

            if (!coordinatorResult.IsSuccess || !coordinatorResult.Value)
            {
                this.SetErrorToast("No tiene permisos para completar formularios en nombre de otros.");
                return RedirectToAction("Index", "Dashboard", new { area = "Participant" });
            }
        }

        // Get project and verify participant has access
        var projectQuery = new LinaSys.BusinessIncubator.Application.Queries.GetProjectByExternalIdQuery(
            projectExternalId,
            CheckAccessForUserId: participantUserId);
        var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);

        if (!projectResult.IsSuccess || projectResult.Value is null)
        {
            return NotFound();
        }

        // Check if user has access through UserProjectAccess (Auth domain)
        if (projectResult.Value.HasAccess == false)
        {
            if (isOnBehalf)
            {
                TempData["ErrorMessage"] = "El participante no tiene acceso a este proyecto.";
            }
            else
            {
                TempData["ErrorMessage"] = "No tienes acceso a este proyecto.";
            }

            return RedirectToAction("Index", "Home");
        }

        // Get form submission status
        var submission = await MediatorExecutor.SendOrThrowAsync(new GetFormSubmissionQuery
        {
            ProjectExternalId = projectExternalId,
            ParticipantUserId = participantUserId!
        });

        // Load feedback if submission exists
        List<FeedbackConversationDto>? feedbackConversations = null;
        var hasPendingFeedback = false;
        if (submission.Id > 0)
        {
            var feedbackQuery = new GetFeedbackForSubmissionQuery(submission.Id, userId);
            var feedbackResult = await MediatorExecutor.SendAndLogIfFailureAsync(feedbackQuery);
            if (feedbackResult.IsSuccess)
            {
                feedbackConversations = feedbackResult.Value;
                hasPendingFeedback = feedbackConversations?.Any(f =>
                    f.Status == LinaSys.BusinessIncubator.Domain.Enums.FeedbackStatus.ReviewNeeded) ?? false;
            }
        }

        // Form fields are read-only when submitted/approved, BUT feedback can still be interactive
        var formFieldsReadOnly = !submission.CanEdit || Request.Query.ContainsKey("readOnly");

        // Feedback should be interactive (NOT read-only) when there's pending feedback
        // Only make feedback read-only if there's no pending feedback to respond to
        var feedbackReadOnly = !hasPendingFeedback;

        // Only show info message for submitted forms being viewed
        if (!submission.CanEdit && submission.Status == "Enviado")
        {
            if (hasPendingFeedback)
            {
                TempData["WarningMessage"] = "Este formulario tiene retroalimentación pendiente de respuesta.";
            }
            else
            {
                TempData["InfoMessage"] = "Este formulario ha sido enviado y está en modo de solo lectura.";
            }
        }
        else if (!submission.CanEdit && submission.Status == "Aprobado")
        {
            TempData["SuccessMessage"] = "Este formulario ha sido aprobado.";
        }
        else if (!submission.CanEdit && submission.Status == "Rechazado")
        {
            TempData["WarningMessage"] = "Este formulario fue rechazado. Puede crear una nueva versión.";
        }

        var model = new ParticipantFormViewModel
        {
            BusinessIncubatorExternalId = businessIncubatorExternalId,
            ProjectExternalId = projectExternalId,
            ProjectId = submission.ProjectId,
            SubmissionId = submission.Id,
            Status = submission.Status,
            StatusCode = submission.StatusCode,
            DraftData = submission.DraftData,
            CanSubmit = submission.CanSubmit && !formFieldsReadOnly,
            IsReadOnly = formFieldsReadOnly,
            FeedbackReadOnly = feedbackReadOnly,
            FeedbackConversations = feedbackConversations ?? new List<FeedbackConversationDto>(),
            IsOnBehalf = isOnBehalf,
            ParticipantUserId = participantUserId,
            CoordinatorUserId = isOnBehalf ? userId : null
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

        // Determine participant user ID based on whether this is on-behalf
        var participantUserId = model.IsOnBehalf && !string.IsNullOrEmpty(model.ParticipantUserId)
            ? model.ParticipantUserId
            : userId;

        // If on-behalf mode, verify coordinator permissions
        if (model.IsOnBehalf)
        {
            var coordinatorQuery = new LinaSys.BusinessIncubator.Application.Queries.IsUserProjectCoordinatorQuery(
                projectExternalId, userId);
            var coordinatorResult = await MediatorExecutor.SendAndLogIfFailureAsync(coordinatorQuery);

            if (!coordinatorResult.IsSuccess || !coordinatorResult.Value)
            {
                return Forbid();
            }
        }

        // For new submissions (submissionId is 0 or not provided), verify project access instead
        if (model.SubmissionId > 0)
        {
            // Verify submission ownership (participant owns it, or coordinator is filling on behalf)
            if (!model.IsOnBehalf)
            {
                var ownershipQuery = new LinaSys.BusinessIncubator.Application.Queries.VerifySubmissionOwnershipQuery(
                    model.ProjectId, model.SubmissionId, participantUserId);
                var ownershipResult = await MediatorExecutor.SendAndLogIfFailureAsync(ownershipQuery);
                if (!ownershipResult.IsSuccess || !ownershipResult.Value)
                {
                    return Forbid();
                }
            }
        }
        else
        {
            // For new submissions, verify the participant has access to the project
            var projectQuery = new LinaSys.BusinessIncubator.Application.Queries.GetProjectByExternalIdQuery(
                projectExternalId,
                CheckAccessForUserId: participantUserId);
            var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);

            if (!projectResult.IsSuccess || projectResult.Value is null || projectResult.Value.HasAccess == false)
            {
                return Forbid();
            }
        }

        // Use the appropriate command based on whether this is on-behalf
        if (model.IsOnBehalf)
        {
            var onBehalfCommand = new LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraftOnBehalf.SaveDraftOnBehalfCommand
            {
                ProjectId = model.ProjectId,
                ParticipantUserId = participantUserId,
                SubmittedByUserId = userId,
                DraftData = model.DraftData
            };

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(onBehalfCommand);

            if (result.IsFailure)
            {
                return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al procesar la solicitud."] });
            }

            return Ok(new { success = true, savedAt = DateTime.UtcNow, submissionId = result.Value, onBehalf = true });
        }
        else
        {
            var command = new SaveDraftCommand
            {
                ProjectId = model.ProjectId,
                ParticipantUserId = participantUserId,
                DraftData = model.DraftData
            };

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

            if (result.IsFailure)
            {
                return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al procesar la solicitud."] });
            }

            return Ok(new { success = true, savedAt = DateTime.UtcNow, submissionId = result.Value });
        }
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

        // For submit, we require a valid submission ID (form must be saved at least once)
        if (model.SubmissionId <= 0)
        {
            return BadRequest(new { errors = new[] { "Debe guardar el formulario antes de enviarlo." } });
        }

        // Verify user owns this submission
        var ownershipQuery = new LinaSys.BusinessIncubator.Application.Queries.VerifySubmissionOwnershipQuery(
            model.ProjectId, model.SubmissionId, userId);
        var ownershipResult = await MediatorExecutor.SendAndLogIfFailureAsync(ownershipQuery);
        if (!ownershipResult.IsSuccess || !ownershipResult.Value)
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
            redirectUrl = Url.Action("Index", "Dashboard", new { area = "Participant" })
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
        [FromRoute] Guid projectExternalId)
    {
        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        // Get project and verify participant has access
        var projectQuery = new LinaSys.BusinessIncubator.Application.Queries.GetProjectByExternalIdQuery(
            projectExternalId,
            CheckAccessForUserId: userId);
        var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);

        if (!projectResult.IsSuccess || projectResult.Value is null)
        {
            return NotFound();
        }

        // Check if user has access through UserProjectAccess (Auth domain)
        if (projectResult.Value.HasAccess == false)
        {
            return Forbid();
        }

        var query = new GetProjectFormStructureQuery
        {
            ProjectId = projectResult.Value.Id
        };

        var result = await MediatorExecutor.SendOrThrowAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Replies to feedback.
    /// </summary>
    /// <param name="businessIncubatorExternalId">The business incubator external ID.</param>
    /// <param name="projectExternalId">The project external ID.</param>
    /// <param name="parentFeedbackId">The parent feedback ID.</param>
    /// <param name="feedbackText">The feedback text.</param>
    /// <returns>The action result.</returns>
    [HttpPost]
    [Route("ReplyToFeedback")]
    public async Task<IActionResult> ReplyToFeedback(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromForm] long parentFeedbackId,
        [FromForm] string feedbackText)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(feedbackText))
        {
            this.SetErrorToast("El texto de la respuesta es requerido.");
            return RedirectToAction(nameof(Index), new { businessIncubatorExternalId, projectExternalId });
        }

        var isParticipant = User.IsInRole(Roles.Starter);
        var command = new ReplyToFeedbackCommand(
            parentFeedbackId,
            feedbackText,
            userId,
            isParticipant);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Respuesta enviada correctamente.");
        }
        else
        {
            this.SetErrorToast("Error al enviar la respuesta.");
        }

        // For AJAX requests, return partial view
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            // Get the submission ID from the form
            var submission = await MediatorExecutor.SendOrThrowAsync(new GetFormSubmissionQuery
            {
                ProjectExternalId = projectExternalId,
                ParticipantUserId = userId
            });

            // Get the updated feedback conversation
            var feedbackQuery = new GetFeedbackForSubmissionQuery(submission.Id, userId);
            var feedbackResult = await MediatorExecutor.SendAndLogIfFailureAsync(feedbackQuery);

            if (feedbackResult.IsSuccess)
            {
                var conversation = feedbackResult.Value?.FirstOrDefault(f => f.Id == parentFeedbackId);
                if (conversation != null)
                {
                    return PartialView("_FeedbackConversation", conversation);
                }
            }
        }

        return RedirectToAction(nameof(Index), new { businessIncubatorExternalId, projectExternalId, anchor = $"feedback-{parentFeedbackId}" });
    }
}
