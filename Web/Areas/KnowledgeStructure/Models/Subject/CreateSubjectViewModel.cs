using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Subject;

public class CreateSubjectViewModel
{
    [Required(ErrorMessage = "El título es requerido.")]
    [Display(Name = "Título")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Contenido")]
    [StringLength(4000, ErrorMessage = "El contenido no puede exceder 4000 caracteres.")]
    public string? Content { get; set; }

    [Required(ErrorMessage = "El tema es requerido.")]
    [Display(Name = "Tema")]
    public long StructureTopicId { get; set; }

    [Display(Name = "Módulo")]
    public long? StructureModuleId { get; set; }

    [Display(Name = "Estructura de Conocimiento")]
    public long? KnowledgeStructureId { get; set; }

    public List<SelectListItem> KnowledgeStructureOptions { get; set; } = [];

    public List<SelectListItem> ModuleOptions { get; set; } = [];

    public List<SelectListItem> TopicOptions { get; set; } = [];
}
