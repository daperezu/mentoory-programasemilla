using LinaSys.Web.Models;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;

/// <summary>
/// View model for the form review dashboard.
/// </summary>
public class FormReviewViewModel
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
    /// Gets or sets the breadcrumb navigation items.
    /// </summary>
    public List<BreadcrumbItem>? Breadcrumbs { get; set; }
}
