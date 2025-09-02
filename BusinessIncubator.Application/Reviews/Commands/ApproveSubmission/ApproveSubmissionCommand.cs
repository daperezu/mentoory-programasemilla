using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Notification.Application.Commands;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Commands.ApproveSubmission;

/// <summary>
/// Command to approve a form submission.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record ApproveSubmissionCommand(
    long SubmissionId,
    string? Comments,
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
}

/// <summary>
/// Handler for ApproveSubmissionCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApproveSubmissionCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="auditContext">The audit context.</param>
/// <param name="mediator">The mediator.</param>
/// <param name="logger">The logger.</param>
public class ApproveSubmissionCommandHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext,
    IMediator mediator,
    ILogger<ApproveSubmissionCommandHandler> logger) : BaseCommandHandler<ApproveSubmissionCommand, ReviewResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<ReviewResultDto>> Handle(
        ApproveSubmissionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get or create review
            var review = await repository.GetReviewBySubmissionIdAsync(request.SubmissionId, cancellationToken);
            if (review is null)
            {
                // Create new review
                review = new Domain.Aggregates.BusinessIncubator.ProjectFormReview(
                    request.SubmissionId,
                    request.ReviewerId,
                    Domain.Enums.ReviewStatus.Approved,
                    DateTime.UtcNow,
                    request.Comments);

                review.CreatedAt = auditContext.UtcNow;
                review.CreatedBy = request.ReviewerId;

                await repository.AddReviewAsync(review, cancellationToken);
            }
            else
            {
                // Update existing review
                review.Approve(request.Comments);
                review.UpdatedAt = auditContext.UtcNow;
                review.UpdatedBy = request.ReviewerId;

                await repository.UpdateReviewAsync(review, cancellationToken);
            }

            // Save changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification email
            await SendApprovalNotificationAsync(request.SubmissionId, request.Comments, cancellationToken);

            logger.LogInformation(
                "Submission {SubmissionId} approved by reviewer {ReviewerId}",
                request.SubmissionId,
                request.ReviewerId);

            return Success(new ReviewResultDto
            {
                ReviewId = review.Id,
                ExternalId = review.ExternalId,
                Status = review.Status.ToString(),
                ReviewedAt = review.ReviewedAt
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving submission {SubmissionId}", request.SubmissionId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(ApproveSubmissionCommand), "Ocurrió un error al aprobar la solicitud."));
        }
    }

    private async Task SendApprovalNotificationAsync(
        long submissionId,
        string? comments,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get submission details
            var submission = await repository.GetSubmissionByIdAsync(submissionId, cancellationToken);
            if (submission is null)
            {
                return;
            }

            // Generate email content
            // TODO: Implement proper email template generation
            var emailContent = $"Su formulario ha sido aprobado.\n\nComentarios: {comments ?? "Sin comentarios"}";

            // Send email
            var sendEmailCommand = new SendEmailCommand(
                submission.ParticipantUserId,
                "Su formulario ha sido aprobado",
                emailContent);

            await mediator.Send(sendEmailCommand, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send approval notification for submission {SubmissionId}", submissionId);
        }
    }
}
