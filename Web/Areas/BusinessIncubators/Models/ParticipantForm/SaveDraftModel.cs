using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ParticipantForm;

/// <summary>
/// Model for save draft requests.
/// </summary>
public class SaveDraftModel
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the form ID.
    /// </summary>
    public long FormId { get; set; }

    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets the draft data.
    /// </summary>
    public DraftDataDto DraftData { get; set; } = null!;
}
