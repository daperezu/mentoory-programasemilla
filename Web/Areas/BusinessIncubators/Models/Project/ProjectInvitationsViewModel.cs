namespace LinaSys.Web.Areas.BusinessIncubators.Models.Project;

/// <summary>
/// View model for managing project invitations.
/// </summary>
public class ProjectInvitationsViewModel
{
    /// <summary>
    /// Gets or sets the business incubator ID.
    /// </summary>
    public Guid BusinessIncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the business incubator name.
    /// </summary>
    public string BusinessIncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;
}
