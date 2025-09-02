namespace LinaSys.KnowledgeStructure.Application.Subject.DTOs;

public class SubjectDetailDto
{
    public long Id { get; set; }

    public long SubjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public long TopicId { get; set; }

    public string TopicName { get; set; } = string.Empty;

    public long ModuleId { get; set; }

    public string ModuleName { get; set; } = string.Empty;

    public long KnowledgeStructureId { get; set; }

    public string KnowledgeStructureName { get; set; } = string.Empty;

    public List<SubjectResourceDto> Resources { get; set; } = [];
}
