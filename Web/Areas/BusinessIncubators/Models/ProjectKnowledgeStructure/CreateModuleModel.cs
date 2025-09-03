using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for creating a new module.
/// </summary>
public class CreateModuleModel
{
    /// <summary>
    /// Gets or sets the name of the module.
    /// </summary>
    [Required(ErrorMessage = "El nombre del módulo es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order of the module.
    /// </summary>
    public int? Order { get; set; }
}
