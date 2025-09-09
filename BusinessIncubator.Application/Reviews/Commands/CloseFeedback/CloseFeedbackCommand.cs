using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Commands.CloseFeedback;

/// <summary>
/// Command to close feedback marking it as resolved.
/// </summary>
public record CloseFeedbackCommand(
    long FeedbackId,
    string UserId) : IBaseRequest;

/// <summary>
/// Handler for CloseFeedbackCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CloseFeedbackCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="timeProvider">The time provider.</param>
/// <param name="logger">The logger.</param>
public class CloseFeedbackCommandHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<CloseFeedbackCommandHandler> logger) : BaseCommandHandler<CloseFeedbackCommand>
{
    /// <inheritdoc/>
    public override async Task<Result> Handle(
        CloseFeedbackCommand request,
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
                    (nameof(CloseFeedbackCommand), $"No se encontró el feedback con ID {request.FeedbackId}."));
            }

            // Close the feedback
            feedback.Close(request.UserId, timeProvider.UtcNow);

            // Update in repository
            repository.UpdateFeedback(feedback);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Feedback {FeedbackId} closed by user {UserId}",
                request.FeedbackId,
                request.UserId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error closing feedback {FeedbackId}", request.FeedbackId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(CloseFeedbackCommand), "Ocurrió un error al cerrar el feedback."));
        }
    }
}