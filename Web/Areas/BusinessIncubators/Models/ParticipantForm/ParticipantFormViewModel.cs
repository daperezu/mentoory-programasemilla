using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectFormStructure;
using LinaSys.BusinessIncubator.Application.Reviews.Queries.GetFeedbackForSubmission;
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
    /// Gets or sets a value indicating whether the form is in read-only mode.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether feedback interactions are read-only.
    /// </summary>
    public bool FeedbackReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the form structure (will be loaded via AJAX).
    /// </summary>
    public ProjectFormStructureDto? FormStructure { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb navigation items.
    /// </summary>
    public List<BreadcrumbItem>? Breadcrumbs { get; set; }

    /// <summary>
    /// Gets or sets the feedback conversations for this submission.
    /// </summary>
    public List<FeedbackConversationDto> FeedbackConversations { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the form is being filled on behalf of another user.
    /// </summary>
    public bool IsOnBehalf { get; set; }

    /// <summary>
    /// Gets or sets the participant user ID (the user for whom the form is being filled).
    /// </summary>
    public string? ParticipantUserId { get; set; }

    /// <summary>
    /// Gets or sets the coordinator user ID (the user filling the form on behalf).
    /// </summary>
    public string? CoordinatorUserId { get; set; }
}
