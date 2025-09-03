using LinaSys.BusinessIncubator.Application.Reviews.Commands.AddFeedback;
using LinaSys.BusinessIncubator.Application.Reviews.Commands.ApproveSubmission;
using LinaSys.BusinessIncubator.Application.Reviews.Commands.RequestChanges;
using LinaSys.BusinessIncubator.Application.Reviews.Queries.GetPendingReviews;
using LinaSys.BusinessIncubator.Application.Reviews.Queries.GetSubmissionForReview;
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
    /// Gets or sets the review ID.
    /// </summary>
    public long ReviewId { get; set; }
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
/// Controller for coordinator form review functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FormReviewController"/> class.
/// </remarks>
/// <param name="mediator">The mediator.</param>
/// <param name="dashboardBuilder">The dashboard builder service.</param>
/// <param name="logger">The logger.</param>
/// <param name="mediatorExecutor">The mediator executor.</param>
/// <param name="repository">The repository.</param>
/// <param name="notificationService">The notification service.</param>
[Area("Coordination")]
[Route("[area]/[controller]")]
public class FormReviewController(
    MediatorExecutor mediator,
    IDashboardBuilderService dashboardBuilder,
    ILogger<FormReviewController> logger,
    MediatorExecutor mediatorExecutor,
    BusinessIncubator.Domain.Repositories.IBusinessIncubatorRepository repository,
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
            request.ReviewId,
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
        // Get review details to find submission ID
        var review = await repository.GetReviewByIdAsync(request.ReviewId, cancellationToken);
        if (review is not null)
        {
            await notificationService.NotifyNewFeedbackAsync(
                projectId,
                review.SubmissionId,
                userName,
                request.FeedbackType.ToString());
        }

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

        var command = new ApproveSubmissionCommand(
            request.SubmissionId,
            request.Comments,
            CurrentUserId);

        var result = await mediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al aprobar el formulario."] });
        }

        TryGetCurrentUserContext(out var contextResult);
        var projectId = contextResult!.ProjectId!.Value;

        // Get submission details for participant name
        var submission = await repository.GetSubmissionByIdAsync(request.SubmissionId, cancellationToken);
        var participantName = submission?.ParticipantUserId ?? "Participante";

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
            data = result.Value
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

    [HttpGet]
    [Route("GetSubmissionDetails/{submissionId:long}")]
    public async Task<IActionResult> GetSubmissionDetails(long submissionId, CancellationToken cancellationToken)
    {
        TryGetCurrentUserContext(out var contextResult);

        // Get the project to obtain its ExternalId
        var project = await repository.GetProjectWithUsersAsync(contextResult!.ProjectId!.Value, cancellationToken);
        if (project is null)
        {
            return BadRequest(new { errors = new[] { "Proyecto no encontrado." } });
        }

        var query = new GetSubmissionForReviewQuery(submissionId, project.ExternalId);
        var result = await mediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al obtener los detalles del formulario."] });
        }

        return Ok(result.Value);
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
        var submission = await repository.GetSubmissionByIdAsync(request.SubmissionId, cancellationToken);
        var participantName = submission?.ParticipantUserId ?? "Participante";

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
    public IActionResult Review(long submissionId)
    {
        TryGetCurrentUserContext(out var contextResult);

        // Set ViewData for layout
        ViewData["DashboardTitle"] = "Revisar Formulario";
        ViewData["BusinessIncubatorId"] = contextResult!.IncubatorId;
        ViewData["ProjectId"] = contextResult.ProjectId;
        ViewData["SubmissionId"] = submissionId;

        return View();
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
