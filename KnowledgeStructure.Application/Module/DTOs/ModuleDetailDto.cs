namespace LinaSys.KnowledgeStructure.Application.Module.DTOs;

public class ModuleDetailDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public long KnowledgeStructureId { get; set; }

    public string KnowledgeStructureName { get; set; } = string.Empty;

    public int TopicCount { get; set; }
}
