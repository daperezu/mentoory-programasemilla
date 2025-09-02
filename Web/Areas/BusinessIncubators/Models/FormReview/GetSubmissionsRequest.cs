namespace LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;

/// <summary>
/// Request model for getting submissions.
/// </summary>
public class GetSubmissionsRequest
{
    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to show only pending submissions.
    /// </summary>
    public bool OnlyPending { get; set; } = true;
}
