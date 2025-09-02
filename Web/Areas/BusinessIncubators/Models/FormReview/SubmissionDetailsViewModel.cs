using LinaSys.Web.Models;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;

/// <summary>
/// View model for submission details.
/// </summary>
public class SubmissionDetailsViewModel
{
    /// <summary>
    /// Gets or sets the business incubator external ID.
    /// </summary>
    public Guid BusinessIncubatorExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project external ID.
    /// </summary>
    public Guid ProjectExternalId { get; set; }

    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets the submission data (loaded via AJAX).
    /// </summary>
    public SubmissionDetailsDto? SubmissionData { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb navigation items.
    /// </summary>
    public List<BreadcrumbItem>? Breadcrumbs { get; set; }
}
