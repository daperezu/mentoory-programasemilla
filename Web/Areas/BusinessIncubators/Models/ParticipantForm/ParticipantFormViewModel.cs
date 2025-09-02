using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectFormStructure;
using LinaSys.Web.Models;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ParticipantForm;

/// <summary>
/// View model for the participant form wizard.
/// </summary>
public class ParticipantFormViewModel
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
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the form ID.
    /// </summary>
    public long FormId { get; set; }

    /// <summary>
    /// Gets or sets the submission ID if it exists.
    /// </summary>
    public long? SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets the submission status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the draft data.
    /// </summary>
    public DraftDataDto? DraftData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the form can be submitted.
    /// </summary>
    public bool CanSubmit { get; set; }

    /// <summary>
    /// Gets or sets the form structure (will be loaded via AJAX).
    /// </summary>
    public ProjectFormStructureDto? FormStructure { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb navigation items.
    /// </summary>
    public List<BreadcrumbItem>? Breadcrumbs { get; set; }
}
