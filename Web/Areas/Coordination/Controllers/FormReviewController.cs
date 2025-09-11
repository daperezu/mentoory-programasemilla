using LinaSys.BusinessIncubator.Application.Reviews.Commands.AddFeedback;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.ApproveWithReview;
using LinaSys.BusinessIncubator.Application.Reviews.Commands.RequestChanges;
using LinaSys.BusinessIncubator.Application.Reviews.Commands.CloseFeedback;
using LinaSys.BusinessIncubator.Application.Reviews.Commands.ReopenFeedback;
using LinaSys.BusinessIncubator.Application.Reviews.Commands.ReplyToFeedback;
using LinaSys.BusinessIncubator.Application.Reviews.Queries.GetPendingReviews;
using LinaSys.BusinessIncubator.Application.Reviews.Queries.GetSubmissionForReview;
using LinaSys.BusinessIncubator.Application.Reviews.Queries.GetAllSubmissionsForReview;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetSubmissionById;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Controllers;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Coordination.Controllers;

/// <summary>
/// Request model for adding feedback.
/// </summary>
public class AddFeedbackRequest
{
    /// <summary>
    /// Gets or sets the block ID (optional).
    /// </summary>
    public long? BlockId { get; set; }

    /// <summary>
    /// Gets or sets the feedback text.
    /// </summary>
    public string FeedbackText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback type.
    /// </summary>
    public FeedbackType FeedbackType { get; set; }

    /// <summary>
    /// Gets or sets the question ID (optional).
    /// </summary>
    public long? QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }
}

/// <summary>
/// Request model for approving a submission.
/// </summary>
public class ApproveRequest
{
    /// <summary>
    /// Gets or sets optional comments.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }
}

/// <summary>
/// Request model for saving coordinator answers.
/// </summary>
public class SaveCoordinatorAnswersRequest
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets the coordinator's draft data.
    /// </summary>
    public BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft.DraftDataDto CoordinatorData { get; set; } = new();

    /// <summary>
    /// Gets or sets preference selections (question IDs where coordinator answer should be used).
    /// </summary>
    public Dictionary<long, bool>? PreferenceSelections { get; set; }
}

/// <summary>
/// Controller for coordinator form review functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FormReviewController"/> class.
/// </remarks>
/// <param name="mediator">The mediator.</param>
/// <param name="dashboardBuilder">The dashboard builder service.</param>
/// <param name="logger">The logger.</param>
/// <param name="mediatorExecutor">The mediator executor.</param>
/// <param name="notificationService">The notification service.</param>
[Area("Coordination")]
[Route("[area]/[controller]")]
public class FormReviewController(
    MediatorExecutor mediator,
    IDashboardBuilderService dashboardBuilder,
    ILogger<FormReviewController> logger,
    MediatorExecutor mediatorExecutor,
    IReviewNotificationService notificationService) : DashboardBaseController(logger, mediator, dashboardBuilder)
{
    [HttpPost]
    [Route("AddFeedback")]
    public async Task<IActionResult> AddFeedback([FromBody] AddFeedbackRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new AddFeedbackCommand(
            request.SubmissionId,  // Changed from ReviewId to SubmissionId
            request.BlockId,
            request.QuestionId,
            request.FeedbackText,
            request.FeedbackType,
            CurrentUserId);

        var result = await mediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al agregar el comentario."] });
        }

        // Send notification about new feedback
        TryGetCurrentUserContext(out var contextResult);

        // Use project ID directly from context
        var projectId = contextResult!.ProjectId!.Value;
        var userName = User.Identity?.Name ?? "Coordinador";
        // Use submission ID directly from request
        await notificationService.NotifyNewFeedbackAsync(
            projectId,
            request.SubmissionId,
            userName,
            request.FeedbackType.ToString());

        return Ok(new { success = true, data = result.Value });
    }

    [HttpPost]
    [Route("Approve")]
    public async Task<IActionResult> Approve([FromBody] ApproveRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current context for project ID
        TryGetCurrentUserContext(out var contextResult);
        var projectId = contextResult!.ProjectId!.Value;

        // Use the new unified command that handles both review and approval
        var command = new ApproveFormSubmissionWithReviewCommand(
            projectId,
            request.SubmissionId,
            CurrentUserId,
            request.Comments);

        var result = await mediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al aprobar el formulario."] });
        }

        // Get submission details for notification
        var submissionQuery = new GetSubmissionByIdQuery(request.SubmissionId);
        var submissionResult = await mediatorExecutor.SendAndLogIfFailureAsync(submissionQuery, cancellationToken);
        var participantName = submissionResult.Value?.ParticipantUserId ?? "Participante";

        await notificationService.NotifyReviewStatusChangeAsync(
            projectId,
            request.SubmissionId,
            "Approved",
            participantName,
            request.Comments);

        return Ok(new
        {
            success = true,
            message = "Formulario aprobado exitosamente. El participante ha sido notificado.",
            redirectUrl = Url.Action("Index", "FormReview", new { area = "Coordination" })
        });
    }

    [HttpPost]
    [Route("GetPendingReviews")]
    public async Task<IActionResult> GetPendingReviews(CancellationToken cancellationToken)
    {
        TryGetCurrentUserContext(out var contextResult);

        var query = new GetPendingReviewsQuery(
            CurrentUserId,
            contextResult!.ProjectId);

        var result = await mediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al obtener las revisiones pendientes."] });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all submissions for review (regardless of status).
    /// </summary>
    [HttpPost]
    [Route("GetAllSubmissions")]
    public async Task<IActionResult> GetAllSubmissions(CancellationToken cancellationToken)
    {
        // Context is required for this operation
        if (!TryGetCurrentUserContext(out var contextResult))
        {
            return BadRequest(new { errors = new[] { "Debe seleccionar un contexto de proyecto para continuar." } });
        }

        // Use the GetAllSubmissionsForReviewQuery to get ALL submissions
        var query = new BusinessIncubator.Application.Reviews.Queries.GetAllSubmissionsForReview.GetAllSubmissionsForReviewQuery(
            CurrentUserId,
            contextResult.ProjectId,
            contextResult.IncubatorId,
            User.IsInRole(Roles.GlobalAdministrator),
            User.IsInRole(Roles.Administrator));

        var result = await mediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al obtener los formularios."] });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [Route("GetSubmissionDetails/{submissionId:long}")]
    public async Task<IActionResult> GetSubmissionDetails(long submissionId, CancellationToken cancellationToken)
    {
        TryGetCurrentUserContext(out var contextResult);

        // Get the project to obtain its ExternalId
        var projectQuery = new GetProjectByIdQuery(contextResult!.ProjectId!.Value);
        var projectResult = await mediatorExecutor.SendAndLogIfFailureAsync(projectQuery, cancellationToken);
        if (projectResult.IsFailure || projectResult.Value is null)
        {
            return BadRequest(new { errors = new[] { "Proyecto no encontrado." } });
        }

        var project = projectResult.Value;

        var query = new GetSubmissionForReviewQuery(submissionId, project.ExternalId);
        var result = await mediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al obtener los detalles del formulario."] });
        }

        // Also get feedback conversations
        var feedbackQuery = new LinaSys.BusinessIncubator.Application.Reviews.Queries.GetFeedbackForSubmission.GetFeedbackForSubmissionQuery(
            submissionId,
            CurrentUserId);
        var feedbackResult = await mediatorExecutor.SendAndLogIfFailureAsync(feedbackQuery, cancellationToken);

        // Add feedback conversations to the response
        var response = new
        {
            submission = result.Value,
            feedbackConversations = feedbackResult.IsSuccess ? feedbackResult.Value : new List<LinaSys.BusinessIncubator.Application.Reviews.Queries.GetFeedbackForSubmission.FeedbackConversationDto>()
        };

        return Ok(response);
    }

    [HttpGet]
    [Route("")]
    public IActionResult Index()
    {
        TryGetCurrentUserContext(out var context);

        // Set ViewData for DashboardBaseController
        ViewData["DashboardTitle"] = "Revisión de Formularios";
        ViewData["BusinessIncubatorId"] = context!.IncubatorId;
        ViewData["ProjectId"] = context.ProjectId;

        return View();
    }

    /// <summary>
    /// Saves coordinator's answers during form review.
    /// </summary>
    [HttpPost]
    [Route("SaveCoordinatorAnswers")]
    public async Task<IActionResult> SaveCoordinatorAnswers([FromBody] SaveCoordinatorAnswersRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveCoordinatorAnswers.SaveCoordinatorAnswersCommand(
            request.SubmissionId,
            CurrentUserId,
            request.CoordinatorData,
            request.PreferenceSelections ?? new Dictionary<long, bool>());

        var result = await mediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al guardar las respuestas del coordinador."] });
        }

        return Ok(new
        {
            success = true,
            message = "Respuestas del coordinador guardadas exitosamente."
        });
    }

    /// <summary>
    /// Gets existing coordinator answers for a submission.
    /// </summary>
    [HttpGet]
    [Route("GetCoordinatorAnswers/{submissionId:long}")]
    public async Task<IActionResult> GetCoordinatorAnswers(long submissionId, CancellationToken cancellationToken)
    {
        // Get submission with coordinator data
        var submissionQuery = new GetSubmissionByIdQuery(submissionId);
        var submissionResult = await mediatorExecutor.SendAndLogIfFailureAsync(submissionQuery, cancellationToken);

        if (submissionResult.IsFailure || submissionResult.Value is null)
        {
            return NotFound(new { error = "Formulario no encontrado." });
        }

        var submission = submissionResult.Value;

        // Parse coordinator data if exists
        if (!string.IsNullOrEmpty(submission.CoordinatorData))
        {
            var coordinatorData = System.Text.Json.JsonSerializer.Deserialize<BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft.DraftDataDto>(
                submission.CoordinatorData,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Extract answers from blocks
            var coordinatorAnswers = new Dictionary<long, string>();
            if (coordinatorData?.BlockResponses != null)
            {
                foreach (var block in coordinatorData.BlockResponses)
                {
                    foreach (var question in block.QuestionResponses)
                    {
                        if (!string.IsNullOrEmpty(question.Answer))
                        {
                            coordinatorAnswers[question.QuestionId] = question.Answer;
                        }
                    }
                }
            }

            return Ok(new
            {
                coordinatorData = coordinatorAnswers,
                preferenceSelections = new Dictionary<long, bool>(), // TODO: Implement preference storage
                coordinatorUserId = submission.CoordinatorUserId,
                coordinatorReviewedAt = submission.CoordinatorReviewedAt
            });
        }

        return Ok(new
        {
            coordinatorData = new Dictionary<long, string>(),
            preferenceSelections = new Dictionary<long, bool>()
        });
    }

    [HttpPost]
    [Route("RequestChanges")]
    public async Task<IActionResult> RequestChanges([FromBody] RequestChangesRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new RequestChangesCommand(
            request.SubmissionId,
            request.Comments,
            request.NewDeadline,
            CurrentUserId);

        var result = await mediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al solicitar cambios."] });
        }

        // Send notification about requested changes
        TryGetCurrentUserContext(out var contextResult);
        var projectId = contextResult!.ProjectId!.Value;

        // Get submission details for participant name
        var submissionQuery = new GetSubmissionByIdQuery(request.SubmissionId);
        var submissionResult = await mediatorExecutor.SendAndLogIfFailureAsync(submissionQuery, cancellationToken);
        var participantName = submissionResult.Value?.ParticipantUserId ?? "Participante";

        await notificationService.NotifyReviewStatusChangeAsync(
            projectId,
            request.SubmissionId,
            "ChangesRequested",
            participantName,
            request.Comments);

        // Also send deadline warning if approaching
        if (request.NewDeadline.Subtract(DateTime.UtcNow).TotalDays <= 7)
        {
            await notificationService.NotifyDeadlineApproachingAsync(
                projectId,
                request.SubmissionId,
                participantName,
                request.NewDeadline);
        }

        return Ok(new
        {
            success = true,
            message = "Se han solicitado cambios. El participante ha sido notificado.",
            data = result.Value
        });
    }

    [HttpGet]
    [Route("Review/{submissionId:long}")]
    public async Task<IActionResult> Review(long submissionId)
    {
        TryGetCurrentUserContext(out var contextResult);

        // Set ViewData for layout
        ViewData["DashboardTitle"] = "Revisar Formulario";
        ViewData["BusinessIncubatorId"] = contextResult!.IncubatorId;
        ViewData["ProjectId"] = contextResult.ProjectId;
        ViewData["SubmissionId"] = submissionId;

        // Load feedback conversations for this submission
        var feedbackQuery = new LinaSys.BusinessIncubator.Application.Reviews.Queries.GetFeedbackForSubmission.GetFeedbackForSubmissionQuery(
            submissionId,
            CurrentUserId);
        var feedbackResult = await mediatorExecutor.SendAndLogIfFailureAsync(feedbackQuery);
        ViewData["FeedbackConversations"] = feedbackResult.IsSuccess ? feedbackResult.Value : new List<LinaSys.BusinessIncubator.Application.Reviews.Queries.GetFeedbackForSubmission.FeedbackConversationDto>();

        return View();
    }

    [HttpPost]
    [Route("ReplyToFeedback")]
    public async Task<IActionResult> ReplyToFeedback(long parentFeedbackId, string feedbackText)
    {
        if (string.IsNullOrWhiteSpace(feedbackText))
        {
            return BadRequest(new { error = "El texto de la respuesta es requerido." });
        }

        var isParticipant = false; // Coordinator is not a participant
        var command = new ReplyToFeedbackCommand(
            parentFeedbackId,
            feedbackText,
            CurrentUserId,
            isParticipant);

        var result = await mediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            return Ok(new { success = true, message = "Respuesta enviada correctamente." });
        }

        return BadRequest(new { error = "Error al enviar la respuesta." });
    }

    [HttpPost]
    [Route("CloseFeedback")]
    public async Task<IActionResult> CloseFeedback(long feedbackId)
    {
        var command = new CloseFeedbackCommand(feedbackId, CurrentUserId);
        var result = await mediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            return Ok(new { success = true, message = "Retroalimentación cerrada correctamente." });
        }

        return BadRequest(new { error = "Error al cerrar la retroalimentación." });
    }

    [HttpPost]
    [Route("ReopenFeedback")]
    public async Task<IActionResult> ReopenFeedback(long feedbackId)
    {
        var command = new ReopenFeedbackCommand(feedbackId, CurrentUserId);
        var result = await mediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            return Ok(new { success = true, message = "Retroalimentación reabierta correctamente." });
        }

        return BadRequest(new { error = "Error al reabrir la retroalimentación." });
    }

    /// <inheritdoc />
    protected override string GetUserRole() => Roles.Coordinator;
}

public class RequestChangesRequest
{
    /// <summary>
    /// Gets or sets the comments.
    /// </summary>
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new deadline.
    /// </summary>
    public DateTime NewDeadline { get; set; }

    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }
}
