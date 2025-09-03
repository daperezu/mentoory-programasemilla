using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for creating a new block.
/// </summary>
public class CreateBlockModel
{
    /// <summary>
    /// Gets or sets the name of the block.
    /// </summary>
    [Required(ErrorMessage = "El nombre del bloque es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order of the block.
    /// </summary>
    public int? Order { get; set; }
}
