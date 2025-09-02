using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.KnowledgeStructure;

public class MoveNodeViewModel
{
    [Required]
    public string NodeId { get; set; } = string.Empty;

    [Required]
    public string NewParentId { get; set; } = string.Empty;

    public int Position { get; set; }
}
