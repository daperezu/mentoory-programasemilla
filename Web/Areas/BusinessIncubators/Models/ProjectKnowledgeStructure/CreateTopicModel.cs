using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for creating a project topic.
/// </summary>
public class CreateTopicModel
{
    /// <summary>
    /// Gets or sets the module ID where the topic will be created.
    /// </summary>
    [Required(ErrorMessage = "El módulo es requerido")]
    public long ModuleId { get; set; }

    /// <summary>
    /// Gets or sets the topic name.
    /// </summary>
    [Required(ErrorMessage = "El nombre del tema es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order.
    /// </summary>
    public int? Order { get; set; }
}
