namespace LinaSys.KnowledgeStructure.Application.Subject.DTOs;

public class SubjectResourceDto
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int? EstimatedMinutes { get; set; }

    public int Order { get; set; }
}
