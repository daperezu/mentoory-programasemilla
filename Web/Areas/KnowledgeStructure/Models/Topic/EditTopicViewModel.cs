using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Topic;

public class EditTopicViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [Display(Name = "Nombre")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string? Description { get; set; }

    public long StructureModuleId { get; set; }

    public string ModuleName { get; set; } = string.Empty;

    public string KnowledgeStructureName { get; set; } = string.Empty;
}
