namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructure;

/// <summary>
/// DTO for project knowledge structure.
/// </summary>
public class ProjectKnowledgeStructureDto
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public long? SourceKnowledgeStructureId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsNameCustomized { get; set; }
    public string? Description { get; set; }
    public bool IsDescriptionCustomized { get; set; }
    public List<ProjectModuleDto> Modules { get; set; } = [];
}

/// <summary>
/// DTO for project module.
/// </summary>
public class ProjectModuleDto
{
    public long Id { get; set; }
    public long? SourceModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsNameCustomized { get; set; }
    public int Order { get; set; }
    public bool IsOrderCustomized { get; set; }
    public List<ProjectTopicDto> Topics { get; set; } = [];
}

/// <summary>
/// DTO for project topic.
/// </summary>
public class ProjectTopicDto
{
    public long Id { get; set; }
    public long? SourceTopicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsNameCustomized { get; set; }
    public int Order { get; set; }
    public bool IsOrderCustomized { get; set; }
    public List<ProjectSubjectDto> Subjects { get; set; } = [];
}

/// <summary>
/// DTO for project subject.
/// </summary>
public class ProjectSubjectDto
{
    public long Id { get; set; }
    public long? SourceSubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsTitleCustomized { get; set; }
    public string? Content { get; set; }
    public bool IsContentCustomized { get; set; }
    public int Order { get; set; }
    public bool IsOrderCustomized { get; set; }
    public List<ProjectSubjectResourceDto> Resources { get; set; } = [];
}

/// <summary>
/// DTO for project subject resource.
/// </summary>
public class ProjectSubjectResourceDto
{
    public long Id { get; set; }
    public long? SourceSubjectResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsTitleCustomized { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsUrlCustomized { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsTypeCustomized { get; set; }
    public int? EstimatedMinutes { get; set; }
    public bool IsEstimatedMinutesCustomized { get; set; }
    public int Order { get; set; }
    public bool IsOrderCustomized { get; set; }
}
