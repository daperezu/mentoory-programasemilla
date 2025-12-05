namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetParticipantSubmissions;

/// <summary>
/// DTO representing a participant's form submission.
/// </summary>
public sealed class ParticipantSubmissionDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the form name.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submission status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CSS class for the status badge.
    /// </summary>
    public string StatusBadgeClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submission date.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the review date.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the rejection reason if applicable.
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the participant can edit this submission.
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the participant can view this submission.
    /// </summary>
    public bool CanView { get; set; }
}
