namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetSubmissionsForReview;

/// <summary>
/// DTO for submissions pending review.
/// </summary>
public class SubmissionForReviewDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the participant user ID.
    /// </summary>
    public string ParticipantUserId { get; set; }

    /// <summary>
    /// Gets or sets the participant name.
    /// </summary>
    public string ParticipantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant email.
    /// </summary>
    public string ParticipantEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form name.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets when the form was started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the form was submitted.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets when the form was reviewed.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the reviewer name if reviewed.
    /// </summary>
    public string? ReviewerName { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public decimal CompletionPercentage { get; set; }
}