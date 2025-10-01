using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Commands.ReopenFeedback;

/// <summary>
/// Command to reopen feedback marking it as needing review.
/// </summary>
public record ReopenFeedbackCommand(
    long FeedbackId,
    string UserId) : IBaseRequest;

/// <summary>
/// Handler for ReopenFeedbackCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReopenFeedbackCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="timeProvider">The time provider.</param>
/// <param name="logger">The logger.</param>
public class ReopenFeedbackCommandHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<ReopenFeedbackCommandHandler> logger) : BaseCommandHandler<ReopenFeedbackCommand>
{
    /// <inheritdoc/>
    public override async Task<Result> Handle(
        ReopenFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the feedback
            var feedback = await repository.GetFeedbackByIdAsync(request.FeedbackId, cancellationToken);
            if (feedback is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(ReopenFeedbackCommand), $"No se encontró el feedback con ID {request.FeedbackId}."));
            }

            // Reopen the feedback
            feedback.Reopen(request.UserId, timeProvider.UtcNow);

            // Update in repository
            repository.UpdateFeedback(feedback);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Feedback {FeedbackId} reopened by user {UserId}",
                request.FeedbackId,
                request.UserId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reopening feedback {FeedbackId}", request.FeedbackId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(ReopenFeedbackCommand), "Ocurrió un error al reabrir el feedback."));
        }
    }
}
