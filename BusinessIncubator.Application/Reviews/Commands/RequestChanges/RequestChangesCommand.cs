using LinaSys.Auth.Application.Queries;
using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Commands.RequestChanges;

/// <summary>
/// Command to request changes for a form submission.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record RequestChangesCommand(
    long SubmissionId,
    string Comments,
    DateTime NewDeadline,
    string ReviewerId) : IBaseRequest<ReviewResultDto>;

/// <summary>
/// Result DTO for review operations.
/// </summary>
public class ReviewResultDto
{
    /// <summary>
    /// Gets or sets the review ID.
    /// </summary>
    public long ReviewId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the review status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the review date.
    /// </summary>
    public DateTime ReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the new deadline.
    /// </summary>
    public DateTime? NewDeadline { get; set; }
}

/// <summary>
/// Handler for RequestChangesCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RequestChangesCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="auditContext">The audit context.</param>
/// <param name="mediator">The mediator.</param>
/// <param name="emailTemplateService">The email template service.</param>
/// <param name="logger">The logger.</param>
public class RequestChangesCommandHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext,
    IMediator mediator,
    ITimeProvider timeProvider,
    ILogger<RequestChangesCommandHandler> logger) : BaseCommandHandler<RequestChangesCommand, ReviewResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<ReviewResultDto>> Handle(
        RequestChangesCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate new deadline
            if (request.NewDeadline <= DateTime.UtcNow)
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(RequestChangesCommand), "La nueva fecha límite debe ser posterior a la fecha actual."));
            }

            // Get or create review
            var review = await repository.GetReviewBySubmissionIdAsync(request.SubmissionId, cancellationToken);
            if (review is null)
            {
                // Create new review
                review = new Domain.Aggregates.BusinessIncubator.ProjectFormReview(
                    request.SubmissionId,
                    request.ReviewerId,
                    Domain.Enums.ReviewStatus.ChangesRequested,
                    DateTime.UtcNow,
                    request.Comments,
                    request.NewDeadline) { CreatedAt = auditContext.UtcNow, CreatedBy = request.ReviewerId };

                await repository.AddReviewAsync(review, cancellationToken);
            }
            else
            {
                // Update existing review
                review.RequestChanges(request.Comments, request.NewDeadline);
                review.UpdatedAt = auditContext.UtcNow;
                review.UpdatedBy = request.ReviewerId;

                await repository.UpdateReviewAsync(review, cancellationToken);
            }

            // Save changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // Publish integration event for review changes requested
            // Get submission details for the event
            var submission = await repository.GetSubmissionByIdAsync(request.SubmissionId, cancellationToken);
            if (submission is not null)
            {
                // Get project information
                var project = await repository.GetProjectByIdAsync(submission.ProjectId, cancellationToken);
                if (project is null)
                {
                    logger.LogWarning("Project {ProjectId} not found for submission {SubmissionId}", submission.ProjectId, submission.Id);
                    return Failure(
                        ResultErrorCodes.Project_NotFound,
                        (nameof(RequestChangesCommand), "El proyecto asociado no fue encontrado."));
                }

                // Get participant user information
                var participantQuery = new GetUserByIdQuery(submission.ParticipantUserId);
                var participantResult = await mediator.Send(participantQuery, cancellationToken);

                string participantName = "Participante";
                string participantEmail = submission.ParticipantUserId; // Fallback to userId

                if (participantResult is { IsSuccess: true, Value: not null })
                {
                    participantName = participantResult.Value.FullName ?? "Participante";
                    participantEmail = participantResult.Value.Email;
                }

                // Get reviewer information
                var reviewerQuery = new GetUserByIdQuery(request.ReviewerId);
                var reviewerResult = await mediator.Send(reviewerQuery, cancellationToken);

                string reviewerName = request.ReviewerId; // Fallback to ID
                if (reviewerResult is { IsSuccess: true, Value: not null })
                {
                    reviewerName = reviewerResult.Value.FullName ?? request.ReviewerId;
                }

                var integrationEvent = new ReviewChangesRequestedIntegrationEvent(
                    SubmissionId: request.SubmissionId,
                    ProjectId: project.Id,
                    ProjectName: project.Name,
                    ParticipantUserId: submission.ParticipantUserId,
                    ParticipantName: participantName,
                    ParticipantEmail: participantEmail,
                    ReviewerName: reviewerName,
                    Feedback: request.Comments,
                    NewDeadline: request.NewDeadline,
                    OccurredOn: timeProvider.UtcNow);

                await mediator.Publish(integrationEvent, cancellationToken);
                logger.LogInformation("Published ReviewChangesRequestedIntegrationEvent for submission {SubmissionId}", submission.Id);
            }

            logger.LogInformation(
                "Changes requested for submission {SubmissionId} by reviewer {ReviewerId}. New deadline: {NewDeadline}",
                request.SubmissionId,
                request.ReviewerId,
                request.NewDeadline);

            return Success(new ReviewResultDto
            {
                ReviewId = review.Id,
                ExternalId = review.ExternalId,
                Status = review.Status.ToString(),
                ReviewedAt = review.ReviewedAt,
                NewDeadline = review.NewDeadline
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting changes for submission {SubmissionId}", request.SubmissionId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(RequestChangesCommand), "Ocurrió un error al solicitar cambios."));
        }
    }
}
