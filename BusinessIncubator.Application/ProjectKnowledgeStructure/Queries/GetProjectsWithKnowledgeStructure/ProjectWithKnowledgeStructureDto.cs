namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectsWithKnowledgeStructure;

/// <summary>
/// DTO for a project with knowledge structure.
/// </summary>
public sealed class ProjectWithKnowledgeStructureDto
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the project external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the knowledge structure name.
    /// </summary>
    public string KnowledgeStructureName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of modules.
    /// </summary>
    public int ModuleCount { get; set; }

    /// <summary>
    /// Gets or sets the number of topics.
    /// </summary>
    public int TopicCount { get; set; }

    /// <summary>
    /// Gets or sets the number of subjects.
    /// </summary>
    public int SubjectCount { get; set; }
}