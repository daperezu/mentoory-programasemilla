using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Commands.AddFeedback;

/// <summary>
/// Command to add feedback to a form submission review.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record AddFeedbackCommand(
    long ReviewId,
    long? BlockId,
    long? QuestionId,
    string FeedbackText,
    FeedbackType FeedbackType,
    string UserId) : IBaseRequest<FeedbackResultDto>;

/// <summary>
/// Result DTO for feedback creation.
/// </summary>
public class FeedbackResultDto
{
    /// <summary>
    /// Gets or sets the feedback ID.
    /// </summary>
    public long FeedbackId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Handler for AddFeedbackCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AddFeedbackCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="logger">The logger.</param>
public class AddFeedbackCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<AddFeedbackCommandHandler> logger) : BaseCommandHandler<AddFeedbackCommand, FeedbackResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<FeedbackResultDto>> Handle(
        AddFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the review
            var review = await repository.GetReviewByIdAsync(request.ReviewId, cancellationToken);
            if (review is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(AddFeedbackCommand), $"La revisión con ID {request.ReviewId} no fue encontrada."));
            }

            // Add feedback
            var feedback = review.AddFeedback(
                request.BlockId,
                request.QuestionId,
                request.FeedbackText,
                request.FeedbackType,
                request.UserId);

            // Save changes
            await repository.UpdateReviewAsync(review, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Feedback added successfully to review {ReviewId} by user {UserId}",
                request.ReviewId,
                request.UserId);

            return Success(new FeedbackResultDto
            {
                FeedbackId = feedback.Id,
                ExternalId = feedback.ExternalId,
                CreatedAt = feedback.CreatedAt
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding feedback to review {ReviewId}", request.ReviewId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(AddFeedbackCommand), "Ocurrió un error al agregar el feedback."));
        }
    }
}
