using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Queries.GetPendingFeedbackCount;

/// <summary>
/// Query to get pending feedback count for a submission.
/// </summary>
public record GetPendingFeedbackCountQuery(
    long SubmissionId,
    string UserId) : IBaseRequest<FeedbackCountDto>;

/// <summary>
/// DTO for feedback count information.
/// </summary>
public class FeedbackCountDto
{
    /// <summary>
    /// Gets or sets the count of feedback needing review.
    /// </summary>
    public int ReviewNeededCount { get; set; }

    /// <summary>
    /// Gets or sets the count of closed feedback.
    /// </summary>
    public int ReviewClosedCount { get; set; }

    /// <summary>
    /// Gets or sets the count of new/unviewed feedback.
    /// </summary>
    public int NewFeedbackCount { get; set; }
}

/// <summary>
/// Handler for GetPendingFeedbackCountQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetPendingFeedbackCountQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="logger">The logger.</param>
public class GetPendingFeedbackCountQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetPendingFeedbackCountQueryHandler> logger) : BaseCommandHandler<GetPendingFeedbackCountQuery, FeedbackCountDto>
{
    /// <inheritdoc/>
    public override async Task<Result<FeedbackCountDto>> Handle(
        GetPendingFeedbackCountQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the submission to verify access
            var submission = await repository.GetFormSubmissionByIdAsync(request.SubmissionId, cancellationToken);
            if (submission is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(GetPendingFeedbackCountQuery), "No se encontró la submisión."));
            }

            // Verify user has access (either participant or coordinator)
            var isParticipant = submission.ParticipantUserId == request.UserId;
            var isCoordinator = await repository.IsUserProjectParticipantAsync(submission.ProjectId, request.UserId, cancellationToken);

            if (!isParticipant && !isCoordinator)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(GetPendingFeedbackCountQuery), "No tiene permisos para ver el feedback de esta submisión."));
            }

            // Get all feedback for this submission (only parent feedback, not replies)
            var allFeedback = await repository.GetFeedbackWithRepliesForSubmissionAsync(request.SubmissionId, cancellationToken);
            var parentFeedback = allFeedback.Where(f => !f.ParentFeedbackId.HasValue).ToList();

            // Count by status
            var reviewNeededCount = parentFeedback.Count(f => f.Status == FeedbackStatus.ReviewNeeded);
            var reviewClosedCount = parentFeedback.Count(f => f.Status == FeedbackStatus.ReviewClosed);

            // For new feedback, we'll consider feedback created in the last 24 hours that hasn't been replied to by the participant
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);
            var newFeedbackCount = 0;

            if (isParticipant)
            {
                // For participants, new feedback is coordinator feedback they haven't replied to
                newFeedbackCount = parentFeedback.Count(f =>
                    !f.IsFromParticipant &&
                    f.CreatedAt > oneDayAgo &&
                    !allFeedback.Any(reply =>
                        reply.ParentFeedbackId == f.Id &&
                        reply.IsFromParticipant));
            }
            else
            {
                // For coordinators, new feedback is participant replies in the last 24 hours
                newFeedbackCount = allFeedback.Count(f =>
                    f.IsFromParticipant &&
                    f.ParentFeedbackId.HasValue &&
                    f.CreatedAt > oneDayAgo);
            }

            var result = new FeedbackCountDto
            {
                ReviewNeededCount = reviewNeededCount,
                ReviewClosedCount = reviewClosedCount,
                NewFeedbackCount = newFeedbackCount
            };

            logger.LogInformation(
                "Feedback count for submission {SubmissionId}: ReviewNeeded={ReviewNeeded}, Closed={Closed}, New={New}",
                request.SubmissionId,
                reviewNeededCount,
                reviewClosedCount,
                newFeedbackCount);

            return Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting feedback count for submission {SubmissionId}", request.SubmissionId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetPendingFeedbackCountQuery), "Ocurrió un error al obtener el conteo de feedback."));
        }
    }
}