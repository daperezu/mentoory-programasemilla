using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for updating a project module.
/// </summary>
public class UpdateModuleModel
{
    /// <summary>
    /// Gets or sets the module name.
    /// </summary>
    [Required(ErrorMessage = "El nombre del módulo es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the module order.
    /// </summary>
    [Required(ErrorMessage = "El orden es requerido")]
    [Range(0, int.MaxValue, ErrorMessage = "El orden debe ser un número positivo")]
    public int Order { get; set; }
}
