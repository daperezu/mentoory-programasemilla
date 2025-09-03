using System.ComponentModel.DataAnnotations;
using LinaSys.Diagnostics.Domain.Enums;

namespace LinaSys.Web.Areas.Diagnostics.Models.Questions;

/// <summary>
/// View model for creating a new question.
/// </summary>
public class CreateQuestionViewModel
{
    [Required(ErrorMessage = "El texto de la pregunta es requerido.")]
    [MaxLength(500, ErrorMessage = "El texto no puede exceder 500 caracteres.")]
    [Display(Name = "Pregunta")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de respuesta es requerido.")]
    [Display(Name = "Tipo de respuesta")]
    public AnswerType AnswerType { get; set; }

    [Required(ErrorMessage = "La fase es requerida.")]
    [Display(Name = "Fase a aplicar")]
    public QuestionPhase AppliesToPhase { get; set; }

    [Display(Name = "¿Se utiliza para el plan de mentoría?")]
    public bool IsUsedForMentoringPlan { get; set; }

    [Display(Name = "¿Se utiliza para diagnóstico?")]
    public bool IsUsedForDiagnosis { get; set; }

    [Display(Name = "Tema")]
    public long? TopicId { get; set; }

    [Display(Name = "Bloque")]
    public long? BlockId { get; set; }

    [Display(Name = "Opciones de respuesta")]
    public List<AnswerOptionViewModel>? AnswerOptions { get; set; }

    // Dropdowns
    public Dictionary<int, string> AnswerTypes { get; set; } = [];
    public Dictionary<long, string> Blocks { get; set; } = [];
    public Dictionary<int, string> QuestionPhases { get; set; } = [];
    public Dictionary<long, QuestionListTopicHierarchyViewModel> Topics { get; set; } = [];
    public Dictionary<long, string> KnowledgeStructures { get; set; } = [];
    public Dictionary<long, QuestionListSubjectHierarchyViewModel> Subjects { get; set; } = [];
}

/// <summary>
/// View model for answer options.
/// </summary>
public class AnswerOptionViewModel
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "El texto de la opción es requerido.")]
    [MaxLength(200, ErrorMessage = "El texto no puede exceder 200 caracteres.")]
    [Display(Name = "Texto de la opción")]
    public string Text { get; set; } = string.Empty;

    [Display(Name = "Puntaje")]
    public int Score { get; set; }

    [Display(Name = "Tipo FODA")]
    public FodaType Foda { get; set; }

    [Required(ErrorMessage = "La explicación FODA es requerida.")]
    [MaxLength(500, ErrorMessage = "La explicación no puede exceder 500 caracteres.")]
    [Display(Name = "Explicación FODA")]
    public string FodaExplanation { get; set; } = string.Empty;

    [Display(Name = "Tipo ODSR")]
    public OdsrType Odsr { get; set; }

    [Required(ErrorMessage = "La explicación ODSR es requerida.")]
    [MaxLength(500, ErrorMessage = "La explicación no puede exceder 500 caracteres.")]
    [Display(Name = "Explicación ODSR")]
    public string OdsrExplanation { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "La pregunta de seguimiento no puede exceder 500 caracteres.")]
    [Display(Name = "Pregunta de seguimiento")]
    public string? FollowupQuestionText { get; set; }

    [Display(Name = "Orden")]
    public int Order { get; set; }
}
