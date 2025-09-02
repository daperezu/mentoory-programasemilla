using System.ComponentModel.DataAnnotations;
using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for copying a knowledge structure to a project.
/// </summary>
public class CopyKnowledgeStructureModel
{
    /// <summary>
    /// Gets or sets the source type (global or project).
    /// </summary>
    [Required(ErrorMessage = "El tipo de origen es requerido")]
    [Range(1, 2, ErrorMessage = "El tipo de origen no es válido")]
    public int SourceType { get; set; }

    /// <summary>
    /// Gets or sets the source form ID.
    /// </summary>
    [Required(ErrorMessage = "El formulario origen es requerido")]
    [Range(1, long.MaxValue, ErrorMessage = "El formulario origen no es válido")]
    public long SourceFormId { get; set; }
}
