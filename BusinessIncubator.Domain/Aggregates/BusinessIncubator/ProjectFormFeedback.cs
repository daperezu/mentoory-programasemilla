using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Represents feedback on a specific part of a form submission.
/// </summary>
public class ProjectFormFeedback : Entity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectFormFeedback"/> class.
    /// </summary>
    /// <param name="reviewId">The review ID.</param>
    /// <param name="blockId">Optional block ID.</param>
    /// <param name="questionId">Optional question ID.</param>
    /// <param name="feedbackText">The feedback text.</param>
    /// <param name="feedbackType">The feedback type.</param>
    public ProjectFormFeedback(
        long reviewId,
        long? blockId,
        long? questionId,
        string feedbackText,
        FeedbackType feedbackType)
    {
        ExternalId = Guid.NewGuid();
        ReviewId = reviewId;
        BlockId = blockId;
        QuestionId = questionId;
        FeedbackText = feedbackText;
        FeedbackType = feedbackType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectFormFeedback"/> class.
    /// </summary>
    protected ProjectFormFeedback()
    {
    }

    /// <summary>
    /// Gets the external ID.
    /// </summary>
    public Guid ExternalId { get; private set; }

    /// <summary>
    /// Gets the review ID.
    /// </summary>
    public long ReviewId { get; private set; }

    /// <summary>
    /// Gets the block ID.
    /// </summary>
    public long? BlockId { get; private set; }

    /// <summary>
    /// Gets the question ID.
    /// </summary>
    public long? QuestionId { get; private set; }

    /// <summary>
    /// Gets the feedback text.
    /// </summary>
    public string FeedbackText { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the feedback type.
    /// </summary>
    public FeedbackType FeedbackType { get; private set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the creator user ID.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the updater user ID.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the deletion date.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the deleter user ID.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Gets the navigation property to the review.
    /// </summary>
    public virtual ProjectFormReview? Review { get; private set; }

    /// <summary>
    /// Updates the feedback text.
    /// </summary>
    /// <param name="feedbackText">The new feedback text.</param>
    /// <param name="feedbackType">The new feedback type.</param>
    public void UpdateFeedback(string feedbackText, FeedbackType feedbackType)
    {
        if (string.IsNullOrWhiteSpace(feedbackText))
        {
            throw new ArgumentException("Feedback text is required.", nameof(feedbackText));
        }

        FeedbackText = feedbackText;
        FeedbackType = feedbackType;
    }
}