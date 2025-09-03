using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for updating a project subject.
/// </summary>
public class UpdateSubjectModel
{
    /// <summary>
    /// Gets or sets the subject name.
    /// </summary>
    [Required(ErrorMessage = "El nombre de la materia es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    [StringLength(2000, ErrorMessage = "El contenido no puede exceder los 2000 caracteres")]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the order.
    /// </summary>
    [Required(ErrorMessage = "El orden es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a 0")]
    public int Order { get; set; }
}
