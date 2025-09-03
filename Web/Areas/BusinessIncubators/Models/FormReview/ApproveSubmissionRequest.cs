namespace LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;

/// <summary>
/// Request model for approving submission.
/// </summary>
public class ApproveSubmissionRequest
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
