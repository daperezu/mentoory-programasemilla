using System.ComponentModel.DataAnnotations;
using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for updating an answer option.
/// </summary>
public class UpdateAnswerOptionModel
{
    /// <summary>
    /// Gets or sets the answer option text.
    /// </summary>
    [Required(ErrorMessage = "El texto de la opción es requerido")]
    [StringLength(1000, ErrorMessage = "El texto no puede exceder los 1000 caracteres")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the score for this answer option.
    /// </summary>
    [Required(ErrorMessage = "El puntaje es requerido")]
    [Range(0, 100, ErrorMessage = "El puntaje debe estar entre 0 y 100")]
    public int Score { get; set; }

    /// <summary>
    /// Gets or sets the FODA type.
    /// </summary>
    [Required(ErrorMessage = "El tipo FODA es requerido")]
    public FodaType Foda { get; set; }

    /// <summary>
    /// Gets or sets the FODA explanation.
    /// </summary>
    [StringLength(500, ErrorMessage = "La explicación FODA no puede exceder los 500 caracteres")]
    public string FodaExplanation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ODSR type.
    /// </summary>
    [Required(ErrorMessage = "El tipo ODSR es requerido")]
    public OdsrType Odsr { get; set; }

    /// <summary>
    /// Gets or sets the ODSR explanation.
    /// </summary>
    [StringLength(500, ErrorMessage = "La explicación ODSR no puede exceder los 500 caracteres")]
    public string OdsrExplanation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    [Required(ErrorMessage = "El orden es requerido")]
    [Range(1, 1000, ErrorMessage = "El orden debe estar entre 1 y 1000")]
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the optional follow-up question text.
    /// </summary>
    [StringLength(1000, ErrorMessage = "La pregunta de seguimiento no puede exceder los 1000 caracteres")]
    public string? FollowUpQuestionText { get; set; }
}
