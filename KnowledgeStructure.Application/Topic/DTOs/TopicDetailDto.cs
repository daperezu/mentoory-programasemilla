namespace LinaSys.KnowledgeStructure.Application.Topic.DTOs;

public class TopicDetailDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public long ModuleId { get; set; }

    public string ModuleName { get; set; } = string.Empty;

    public long KnowledgeStructureId { get; set; }

    public string KnowledgeStructureName { get; set; } = string.Empty;

    public int SubjectCount { get; set; }
}
