using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Commands.ReplyToFeedback;

/// <summary>
/// Command to reply to existing feedback.
/// </summary>
public record ReplyToFeedbackCommand(
    long ParentFeedbackId,
    string FeedbackText,
    string UserId,
    bool IsFromParticipant) : IBaseRequest<FeedbackDto>;

/// <summary>
/// DTO for feedback information.
/// </summary>
public class FeedbackDto
{
    /// <summary>
    /// Gets or sets the feedback ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the feedback text.
    /// </summary>
    public string FeedbackText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback type.
    /// </summary>
    public FeedbackType FeedbackType { get; set; }

    /// <summary>
    /// Gets or sets the feedback status.
    /// </summary>
    public FeedbackStatus Status { get; set; }

    /// <summary>
    /// Gets or sets whether the feedback is from a participant.
    /// </summary>
    public bool IsFromParticipant { get; set; }

    /// <summary>
    /// Gets or sets the created by user ID.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Handler for ReplyToFeedbackCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReplyToFeedbackCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="timeProvider">The time provider.</param>
/// <param name="logger">The logger.</param>
public class ReplyToFeedbackCommandHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<ReplyToFeedbackCommandHandler> logger) : BaseCommandHandler<ReplyToFeedbackCommand, FeedbackDto>
{
    /// <inheritdoc/>
    public override async Task<Result<FeedbackDto>> Handle(
        ReplyToFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the parent feedback
            var parentFeedback = await repository.GetFeedbackByIdAsync(request.ParentFeedbackId, cancellationToken);
            if (parentFeedback is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(ReplyToFeedbackCommand), $"No se encontró el feedback con ID {request.ParentFeedbackId}."));
            }

            // Create the reply
            var reply = parentFeedback.Reply(
                request.FeedbackText,
                request.UserId,
                request.IsFromParticipant,
                timeProvider.UtcNow);

            // Save the reply
            await repository.AddFeedbackAsync(reply, cancellationToken);

            // Update parent if it was reopened
            if (parentFeedback.Status == FeedbackStatus.ReviewNeeded)
            {
                repository.UpdateFeedback(parentFeedback);
            }

            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Reply added to feedback {ParentFeedbackId} by user {UserId} (IsParticipant: {IsFromParticipant})",
                request.ParentFeedbackId,
                request.UserId,
                request.IsFromParticipant);

            return Success(new FeedbackDto
            {
                Id = reply.Id,
                ExternalId = reply.ExternalId,
                FeedbackText = reply.FeedbackText,
                FeedbackType = reply.FeedbackType,
                Status = reply.Status,
                IsFromParticipant = reply.IsFromParticipant,
                CreatedBy = reply.CreatedBy,
                CreatedAt = reply.CreatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation when replying to feedback {ParentFeedbackId}", request.ParentFeedbackId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(ReplyToFeedbackCommand), ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error replying to feedback {ParentFeedbackId}", request.ParentFeedbackId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(ReplyToFeedbackCommand), "Ocurrió un error al responder al feedback."));
        }
    }
}