using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for creating a project subject.
/// </summary>
public class CreateSubjectModel
{
    /// <summary>
    /// Gets or sets the topic ID where the subject will be created.
    /// </summary>
    [Required(ErrorMessage = "El tema es requerido")]
    public long TopicId { get; set; }

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
    public int? Order { get; set; }
}
