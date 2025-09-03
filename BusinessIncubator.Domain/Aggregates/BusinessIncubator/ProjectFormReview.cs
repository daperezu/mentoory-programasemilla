using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Represents a review of a project form submission.
/// </summary>
public class ProjectFormReview : Entity
{
    private readonly List<ProjectFormFeedback> _feedbackItems = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectFormReview"/> class.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="reviewerId">The reviewer user ID.</param>
    /// <param name="status">The review status.</param>
    /// <param name="reviewedAt">The review date.</param>
    /// <param name="generalComments">Optional general comments.</param>
    /// <param name="newDeadline">Optional new deadline for changes.</param>
    public ProjectFormReview(
        long submissionId,
        string reviewerId,
        ReviewStatus status,
        DateTime reviewedAt,
        string? generalComments = null,
        DateTime? newDeadline = null)
    {
        ExternalId = Guid.NewGuid();
        SubmissionId = submissionId;
        ReviewerId = reviewerId;
        Status = status;
        ReviewedAt = reviewedAt;
        GeneralComments = generalComments;
        NewDeadline = newDeadline;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectFormReview"/> class.
    /// </summary>
    protected ProjectFormReview()
    {
    }

    /// <summary>
    /// Gets the external ID.
    /// </summary>
    public Guid ExternalId { get; private set; }

    /// <summary>
    /// Gets the submission ID.
    /// </summary>
    public long SubmissionId { get; private set; }

    /// <summary>
    /// Gets the reviewer user ID.
    /// </summary>
    public string ReviewerId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the review status.
    /// </summary>
    public ReviewStatus Status { get; private set; }

    /// <summary>
    /// Gets the review date.
    /// </summary>
    public DateTime ReviewedAt { get; private set; }

    /// <summary>
    /// Gets the general comments.
    /// </summary>
    public string? GeneralComments { get; private set; }

    /// <summary>
    /// Gets the new deadline for changes.
    /// </summary>
    public DateTime? NewDeadline { get; private set; }

    /// <summary>
    /// Gets the feedback items.
    /// </summary>
    public IReadOnlyList<ProjectFormFeedback> FeedbackItems => _feedbackItems.AsReadOnly();

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
    /// Gets the navigation property to the submission.
    /// </summary>
    public virtual ProjectFormSubmission? Submission { get; private set; }

    /// <summary>
    /// Adds feedback to the review.
    /// </summary>
    /// <param name="blockId">Optional block ID.</param>
    /// <param name="questionId">Optional question ID.</param>
    /// <param name="feedbackText">The feedback text.</param>
    /// <param name="feedbackType">The feedback type.</param>
    /// <param name="createdBy">The user creating the feedback.</param>
    /// <returns>The created feedback.</returns>
    public ProjectFormFeedback AddFeedback(
        long? blockId,
        long? questionId,
        string feedbackText,
        FeedbackType feedbackType,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(feedbackText))
        {
            throw new ArgumentException("Feedback text is required.", nameof(feedbackText));
        }

        var feedback = new ProjectFormFeedback(
            this.Id,
            blockId,
            questionId,
            feedbackText,
            feedbackType);

        feedback.CreatedAt = DateTime.UtcNow;
        feedback.CreatedBy = createdBy;

        _feedbackItems.Add(feedback);
        return feedback;
    }

    /// <summary>
    /// Updates the review status.
    /// </summary>
    /// <param name="status">The new status.</param>
    /// <param name="generalComments">Optional general comments.</param>
    /// <param name="newDeadline">Optional new deadline.</param>
    public void UpdateStatus(ReviewStatus status, string? generalComments = null, DateTime? newDeadline = null)
    {
        Status = status;
        ReviewedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(generalComments))
        {
            GeneralComments = generalComments;
        }

        if (status == ReviewStatus.ChangesRequested && newDeadline.HasValue)
        {
            NewDeadline = newDeadline.Value;
        }
    }

    /// <summary>
    /// Approves the submission.
    /// </summary>
    /// <param name="comments">Optional approval comments.</param>
    public void Approve(string? comments = null)
    {
        UpdateStatus(ReviewStatus.Approved, comments);
    }

    /// <summary>
    /// Requests changes for the submission.
    /// </summary>
    /// <param name="comments">Comments explaining required changes.</param>
    /// <param name="newDeadline">New deadline for resubmission.</param>
    public void RequestChanges(string comments, DateTime newDeadline)
    {
        if (string.IsNullOrWhiteSpace(comments))
        {
            throw new ArgumentException("Comments are required when requesting changes.", nameof(comments));
        }

        UpdateStatus(ReviewStatus.ChangesRequested, comments, newDeadline);
    }

    /// <summary>
    /// Flags the submission for special attention.
    /// </summary>
    /// <param name="reason">Reason for flagging.</param>
    public void Flag(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required when flagging.", nameof(reason));
        }

        UpdateStatus(ReviewStatus.Flagged, reason);
    }
}