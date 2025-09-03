using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Module;

public class CreateModuleViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [Display(Name = "Nombre del módulo")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La estructura de conocimiento es requerida")]
    [Display(Name = "Estructura de conocimiento")]
    public long KnowledgeStructureId { get; set; }

    [Display(Name = "Orden")]
    [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a 0")]
    public int Order { get; set; } = 1;

    public List<SelectListItem> KnowledgeStructureOptions { get; set; } = [];
}
