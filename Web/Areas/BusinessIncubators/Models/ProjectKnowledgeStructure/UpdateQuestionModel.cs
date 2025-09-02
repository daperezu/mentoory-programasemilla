using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for updating a project question.
/// </summary>
public class UpdateQuestionModel
{
    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    [Required(ErrorMessage = "El texto de la pregunta es requerido")]
    [StringLength(1000, ErrorMessage = "El texto no puede exceder los 1000 caracteres")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the answer type.
    /// </summary>
    [Required(ErrorMessage = "El tipo de respuesta es requerido")]
    public int AnswerType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the question is used for diagnosis.
    /// </summary>
    public bool IsUsedForDiagnosis { get; set; }

    /// <summary>
    /// Gets or sets the phase the question applies to.
    /// </summary>
    [Required(ErrorMessage = "La fase es requerida")]
    public int AppliesToPhase { get; set; }

    /// <summary>
    /// Gets or sets the order.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a 0")]
    public int? Order { get; set; }

    /// <summary>
    /// Gets or sets the optional topic ID to link the question to.
    /// </summary>
    public long? TopicId { get; set; }
}
