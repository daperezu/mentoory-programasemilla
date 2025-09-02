using LinaSys.Diagnostics.Domain.Enums;

namespace LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;

/// <summary>
/// Input model for creating diagnosis answers within the aggregate.
/// </summary>
public class DiagnosisAnswerInput
{
    /// <summary>
    /// Gets or sets the answer option identifier.
    /// </summary>
    public long AnswerOptionId { get; set; }

    /// <summary>
    /// Gets or sets the answer option text.
    /// </summary>
    public string AnswerOptionText { get; set; }

    /// <summary>
    /// Gets or sets the answer option user input.
    /// </summary>
    public string AnswerOptionUserInput { get; set; }

    /// <summary>
    /// Gets or sets the block identifier.
    /// </summary>
    public long BlockId { get; set; }

    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string BlockName { get; set; }

    /// <summary>
    /// Gets or sets the FODA type.
    /// </summary>
    public FodaType Foda { get; set; }

    /// <summary>
    /// Gets or sets the FODA explanation.
    /// </summary>
    public string FodaExplanation { get; set; }

    /// <summary>
    /// Gets or sets the follow-up answer user input.
    /// </summary>
    public string FollowUpAnswerUserInput { get; set; }

    /// <summary>
    /// Gets or sets the follow-up question text.
    /// </summary>
    public string FollowUpQuestionText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this answer is used for diagnosis.
    /// </summary>
    public bool IsUsedForDiagnosis { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this answer is used for mentoring plan.
    /// </summary>
    public bool IsUsedForMentoringPlan { get; set; }

    /// <summary>
    /// Gets or sets the module identifier.
    /// </summary>
    public long? ModuleId { get; set; }

    /// <summary>
    /// Gets or sets the module name.
    /// </summary>
    public string ModuleName { get; set; }

    /// <summary>
    /// Gets or sets the ODSR type.
    /// </summary>
    public OdsrType Odsr { get; set; }

    /// <summary>
    /// Gets or sets the ODSR explanation.
    /// </summary>
    public string OdsrExplanation { get; set; }

    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the question identifier.
    /// </summary>
    public long QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    public string QuestionText { get; set; }

    /// <summary>
    /// Gets or sets the score.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Gets or sets the topic identifier.
    /// </summary>
    public long? TopicId { get; set; }

    /// <summary>
    /// Gets or sets the topic name.
    /// </summary>
    public string TopicName { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string UserId { get; set; }
}
