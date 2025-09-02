namespace LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;

/// <summary>
/// Request model for rejecting submission.
/// </summary>
public class RejectSubmissionRequest
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
    /// Gets or sets the rejection reason.
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;
}
