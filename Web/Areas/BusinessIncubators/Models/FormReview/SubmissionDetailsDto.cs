namespace LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;

/// <summary>
/// DTO for submission details.
/// </summary>
public class SubmissionDetailsDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

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
    /// Gets or sets the submission status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the form was started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the form was submitted.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the answers grouped by block.
    /// </summary>
    public List<BlockAnswersDto> BlockAnswers { get; set; } = [];
}
