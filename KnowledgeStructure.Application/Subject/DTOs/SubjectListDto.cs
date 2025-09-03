namespace LinaSys.KnowledgeStructure.Application.Subject.DTOs;

public class SubjectListDto
{
    public long Id { get; set; }

    public long SubjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string TopicName { get; set; } = string.Empty;

    public string ModuleName { get; set; } = string.Empty;

    public string KnowledgeStructureName { get; set; } = string.Empty;

    public int ResourceCount { get; set; }

    public int Order { get; set; }
}
