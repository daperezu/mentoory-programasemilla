using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Subject;

public class EditSubjectViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "El título es requerido.")]
    [Display(Name = "Título")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Contenido")]
    [StringLength(4000, ErrorMessage = "El contenido no puede exceder 4000 caracteres.")]
    public string? Content { get; set; }

    public long StructureTopicId { get; set; }

    public string TopicName { get; set; } = string.Empty;

    public string ModuleName { get; set; } = string.Empty;

    public string KnowledgeStructureName { get; set; } = string.Empty;
}
