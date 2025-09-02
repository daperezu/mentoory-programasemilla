namespace LinaSys.Web.Areas.KnowledgeStructure.Models.KnowledgeStructure;

#pragma warning disable SA1300 // Element should begin with upper-case letter (required for jsTree JSON format)

public class TreeNodeViewModel
{
    public string id { get; set; } = string.Empty;

    public string text { get; set; } = string.Empty;

    public string icon { get; set; } = string.Empty;

    public string type { get; set; } = string.Empty;

    public object? data { get; set; }

    public bool children { get; set; }
}
