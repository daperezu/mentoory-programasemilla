using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Subject;

public class AddSubjectResourceViewModel
{
    [Required]
    public long SubjectId { get; set; }

    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "La URL es requerida")]
    [StringLength(500, ErrorMessage = "La URL no puede exceder los 500 caracteres")]
    [Url(ErrorMessage = "La URL no es válida")]
    public string Url { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es requerido")]
    [StringLength(50, ErrorMessage = "El tipo no puede exceder los 50 caracteres")]
    public string Type { get; set; } = string.Empty;

    [Range(1, 9999, ErrorMessage = "La duración estimada debe estar entre 1 y 9999 minutos")]
    public int? EstimatedMinutes { get; set; }
}
