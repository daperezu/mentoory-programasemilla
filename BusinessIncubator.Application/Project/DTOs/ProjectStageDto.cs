using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.BusinessIncubator.Application.Project.DTOs;

/// <summary>
/// Data transfer object for project stage information.
/// </summary>
public class ProjectStageDto
{
    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets who created the stage.
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the days remaining until the stage ends.
    /// </summary>
    public int DaysRemaining { get; set; }

    /// <summary>
    /// Gets or sets the stage description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the stage ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the stage is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this is the current stage.
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the stage title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the stage type.
    /// </summary>
    public ProjectStageType Type { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who last updated the stage.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
