using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetParticipantSubmissions;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.Projects;

/// <summary>
/// View model for the participant dashboard.
/// </summary>
public class ParticipantDashboardViewModel
{
    /// <summary>
    /// Gets or sets the business incubator external ID.
    /// </summary>
    public Guid BusinessIncubatorExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant's form submissions.
    /// </summary>
    public List<ParticipantSubmissionDto> FormSubmissions { get; set; } = [];
}
