namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// View model for the project knowledge structure page.
/// </summary>
public class ProjectKnowledgeStructureViewModel
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
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the project has a knowledge structure.
    /// </summary>
    public bool HasKnowledgeStructure { get; set; }

    /// <summary>
    /// Gets or sets the source form ID if the structure was copied.
    /// </summary>
    public long? SourceFormId { get; set; }

    /// <summary>
    /// Gets or sets the source form name if available.
    /// </summary>
    public string? SourceFormName { get; set; }
}
