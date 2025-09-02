namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.DTOs;

public class KnowledgeStructureDetailDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int ModuleCount { get; set; }

    public DateTime CreatedAt { get; set; }
}
