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
}
