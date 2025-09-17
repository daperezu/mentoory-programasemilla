namespace LinaSys.Web.Areas.BusinessIncubators.Models.ParticipantForm;

/// <summary>
/// Model for submit form requests.
/// </summary>
public class SubmitFormModel
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets whether this submission is being done on behalf of another user.
    /// </summary>
    public bool IsOnBehalf { get; set; }

    /// <summary>
    /// Gets or sets the participant user ID when submitting on behalf.
    /// </summary>
    public string? ParticipantUserId { get; set; }
}
