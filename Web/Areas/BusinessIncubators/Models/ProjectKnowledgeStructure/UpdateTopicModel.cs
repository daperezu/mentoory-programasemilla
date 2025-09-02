using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for updating a project topic.
/// </summary>
public class UpdateTopicModel
{
    /// <summary>
    /// Gets or sets the topic name.
    /// </summary>
    [Required(ErrorMessage = "El nombre del tema es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order.
    /// </summary>
    [Required(ErrorMessage = "El orden es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a 0")]
    public int Order { get; set; }
}
