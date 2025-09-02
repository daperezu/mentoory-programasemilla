namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Module;

public class ModuleListViewModel
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string KnowledgeStructureName { get; set; } = string.Empty;

    public int Order { get; set; }

    public int TopicCount { get; set; }
}
