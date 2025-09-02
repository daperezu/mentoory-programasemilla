using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Queries.GetSubmissionForReview;

/// <summary>
/// Query to get a submission for review with all details.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetSubmissionForReviewQuery(
    long SubmissionId,
    Guid ProjectId) : IBaseRequest<SubmissionReviewDto>;

/// <summary>
/// DTO for submission review details.
/// </summary>
public class SubmissionReviewDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form name.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form version.
    /// </summary>
    public int FormVersion { get; set; }

    /// <summary>
    /// Gets or sets the submission status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submission date.
    /// </summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the blocks.
    /// </summary>
    public List<BlockReviewDto> Blocks { get; set; } = [];

    /// <summary>
    /// Gets or sets existing reviews.
    /// </summary>
    public List<ReviewHistoryDto> ReviewHistory { get; set; } = [];

    /// <summary>
    /// Gets or sets the current review status.
    /// </summary>
    public string? CurrentReviewStatus { get; set; }

    /// <summary>
    /// Gets or sets the current review comments.
    /// </summary>
    public string? CurrentReviewComments { get; set; }

    /// <summary>
    /// Gets or sets the current review deadline.
    /// </summary>
    public DateTime? CurrentReviewDeadline { get; set; }
}

/// <summary>
/// DTO for block review information.
/// </summary>
public class BlockReviewDto
{
    /// <summary>
    /// Gets or sets the block ID.
    /// </summary>
    public long BlockId { get; set; }

    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the block description.
    /// </summary>
    public string? BlockDescription { get; set; }

    /// <summary>
    /// Gets or sets the block order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the questions.
    /// </summary>
    public List<QuestionReviewDto> Questions { get; set; } = [];

    /// <summary>
    /// Gets or sets the feedback items for this block.
    /// </summary>
    public List<FeedbackItemDto> Feedback { get; set; } = [];
}

/// <summary>
/// DTO for question review information.
/// </summary>
public class QuestionReviewDto
{
    /// <summary>
    /// Gets or sets the question ID.
    /// </summary>
    public long QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the question type.
    /// </summary>
    public string QuestionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the question is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the user's answer.
    /// </summary>
    public string? Answer { get; set; }

    /// <summary>
    /// Gets or sets the feedback items for this question.
    /// </summary>
    public List<FeedbackItemDto> Feedback { get; set; } = [];
}

/// <summary>
/// DTO for feedback items.
/// </summary>
public class FeedbackItemDto
{
    /// <summary>
    /// Gets or sets the feedback ID.
    /// </summary>
    public long FeedbackId { get; set; }

    /// <summary>
    /// Gets or sets the feedback text.
    /// </summary>
    public string FeedbackText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback type.
    /// </summary>
    public string FeedbackType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reviewer name.
    /// </summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for review history.
/// </summary>
public class ReviewHistoryDto
{
    /// <summary>
    /// Gets or sets the review ID.
    /// </summary>
    public long ReviewId { get; set; }

    /// <summary>
    /// Gets or sets the reviewer name.
    /// </summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the review status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the review date.
    /// </summary>
    public DateTime ReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the general comments.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Gets or sets the feedback count.
    /// </summary>
    public int FeedbackCount { get; set; }
}

/// <summary>
/// Handler for GetSubmissionForReviewQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetSubmissionForReviewQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="logger">The logger.</param>
public class GetSubmissionForReviewQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetSubmissionForReviewQueryHandler> logger) : BaseCommandHandler<GetSubmissionForReviewQuery, SubmissionReviewDto>
{

    /// <inheritdoc/>
    public override async Task<Result<SubmissionReviewDto>> Handle(
        GetSubmissionForReviewQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get submission with all related data
            var submission = await repository.GetSubmissionWithDetailsForReviewAsync(
                request.SubmissionId,
                cancellationToken);

            if (submission is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(GetSubmissionForReviewQuery), $"La solicitud con ID {request.SubmissionId} no fue encontrada."));
            }

            // Authorization is now handled by the RequiresPermission attribute and AuthorizationBehavior
            // The ProjectId in the query will be used to verify access via ProtectedResource

            // Map to DTO
            var dto = new SubmissionReviewDto
            {
                SubmissionId = submission.Id,
                ExternalId = Guid.NewGuid(), // TODO: Add ExternalId to ProjectFormSubmission if needed
                UserId = submission.ParticipantUserId,
                UserName = "Usuario", // TODO: Get user name from UserManager if needed
                UserEmail = string.Empty, // TODO: Get user email from UserManager if needed
                FormName = "Formulario", // TODO: Get form name from ProjectForm if needed
                FormVersion = submission.FormSchemaVersion,
                Status = submission.Status.ToString(),
                SubmittedAt = submission.SubmittedAt ?? submission.StartedAt,
                CompletionPercentage = 0 // TODO: Calculate completion percentage if needed
            };

            // TODO: Map blocks and questions from draft data if needed
            // For now, returning empty blocks
            dto.Blocks = [];

            // Get review history
            var reviews = await repository.GetReviewsBySubmissionIdAsync(
                request.SubmissionId,
                cancellationToken);

            dto.ReviewHistory = reviews.Select(r => new ReviewHistoryDto
            {
                ReviewId = r.Id,
                ReviewerName = r.ReviewerId, // TODO: Get reviewer name from UserManager if needed
                Status = r.Status.ToString(),
                ReviewedAt = r.ReviewedAt,
                Comments = r.GeneralComments,
                FeedbackCount = r.FeedbackItems.Count
            }).OrderByDescending(r => r.ReviewedAt).ToList();

            // Get current review status
            var currentReview = reviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault();
            if (currentReview is not null)
            {
                dto.CurrentReviewStatus = currentReview.Status.ToString();
                dto.CurrentReviewComments = currentReview.GeneralComments;
                dto.CurrentReviewDeadline = currentReview.NewDeadline;

                // TODO: Map feedback to blocks and questions if needed
            }

            logger.LogInformation(
                "Retrieved submission {SubmissionId} for review",
                request.SubmissionId);

            return Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving submission {SubmissionId} for review", request.SubmissionId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetSubmissionForReviewQuery), "Ocurrió un error al obtener la solicitud para revisión."));
        }
    }

    // TODO: Implement mapping methods when proper navigation properties are available
    // These methods will need to parse DraftData JSON and map to DTOs
}
