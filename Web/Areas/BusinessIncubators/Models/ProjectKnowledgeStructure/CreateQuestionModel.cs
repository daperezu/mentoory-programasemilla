using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for creating a new question.
/// </summary>
public class CreateQuestionModel
{
    /// <summary>
    /// Gets or sets the block ID where the question will be added.
    /// </summary>
    [Required(ErrorMessage = "El bloque es requerido")]
    public long BlockId { get; set; }

    /// <summary>
    /// Gets or sets the text of the question.
    /// </summary>
    [Required(ErrorMessage = "El texto de la pregunta es requerido")]
    [StringLength(1000, ErrorMessage = "El texto no puede exceder 1000 caracteres")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the answer type.
    /// </summary>
    [Required(ErrorMessage = "El tipo de respuesta es requerido")]
    public int AnswerType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this question is used for diagnosis.
    /// </summary>
    public bool IsUsedForDiagnosis { get; set; }

    /// <summary>
    /// Gets or sets the phase this question applies to.
    /// </summary>
    [Required(ErrorMessage = "La fase es requerida")]
    public int AppliesToPhase { get; set; }

    /// <summary>
    /// Gets or sets the order of the question.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Gets or sets the optional topic ID.
    /// </summary>
    public long? TopicId { get; set; }
}
