namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Topic;

public class TopicListViewModel
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ModuleName { get; set; } = string.Empty;

    public string KnowledgeStructureName { get; set; } = string.Empty;

    public int SubjectCount { get; set; }
}
