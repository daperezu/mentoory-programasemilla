using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Topic;

public class CreateTopicViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [Display(Name = "Nombre")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El módulo es requerido.")]
    [Display(Name = "Módulo")]
    public long StructureModuleId { get; set; }

    [Display(Name = "Estructura de Conocimiento")]
    public long? KnowledgeStructureId { get; set; }

    public List<SelectListItem> KnowledgeStructureOptions { get; set; } = [];

    public List<SelectListItem> ModuleOptions { get; set; } = [];
}
