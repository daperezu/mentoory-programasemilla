using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;

/// <summary>
/// Represents a single answer within a user's diagnosis.
/// This is an entity within the UserProjectDiagnosis aggregate.
/// </summary>
public class DiagnosisAnswer : Entity
{
    protected DiagnosisAnswer()
    {
        // Required by EF Core
    }

    private DiagnosisAnswer(
        DiagnosisAnswerInput input,
        QuestionPhase phase,
        int order,
        DateTime submittedAt)
    {
        // Map all properties from input
        ProjectId = input.ProjectId;
        UserId = input.UserId;
        TopicId = input.TopicId;
        TopicName = input.TopicName;
        ModuleId = input.ModuleId;
        ModuleName = input.ModuleName;
        BlockId = input.BlockId;
        BlockName = input.BlockName;
        QuestionId = input.QuestionId;
        QuestionText = input.QuestionText;
        AnswerOptionId = input.AnswerOptionId;
        AnswerOptionText = input.AnswerOptionText;
        AnswerOptionUserInput = input.AnswerOptionUserInput;
        FollowUpQuestionText = input.FollowUpQuestionText;
        FollowUpAnswerUserInput = input.FollowUpAnswerUserInput;
        Score = input.Score;
        Foda = input.Foda;
        FodaExplanation = input.FodaExplanation;
        Odsr = input.Odsr;
        OdsrExplanation = input.OdsrExplanation;
        Phase = phase;
        IsUsedForMentoringPlan = input.IsUsedForMentoringPlan;
        IsUsedForDiagnosis = input.IsUsedForDiagnosis;
        Order = order;
        SubmittedAt = submittedAt;
        AnswerSource = input.AnswerSource;
        CoordinatorUserId = input.CoordinatorUserId;
        PreferredForDiagnosis = input.PreferredForDiagnosis;
    }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Gets the module identifier.
    /// </summary>
    public long? ModuleId { get; private set; }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public string ModuleName { get; private set; }

    /// <summary>
    /// Gets the topic identifier.
    /// </summary>
    public long? TopicId { get; private set; }

    /// <summary>
    /// Gets the topic name.
    /// </summary>
    public string TopicName { get; private set; }

    /// <summary>
    /// Gets the block identifier.
    /// </summary>
    public long BlockId { get; private set; }

    /// <summary>
    /// Gets the block name.
    /// </summary>
    public string BlockName { get; private set; }

    /// <summary>
    /// Gets the question identifier.
    /// </summary>
    public long QuestionId { get; private set; }

    /// <summary>
    /// Gets the question text.
    /// </summary>
    public string QuestionText { get; private set; }

    /// <summary>
    /// Gets the answer option identifier.
    /// </summary>
    public long AnswerOptionId { get; private set; }

    /// <summary>
    /// Gets the answer option text.
    /// </summary>
    public string AnswerOptionText { get; private set; }

    /// <summary>
    /// Gets the answer option user input.
    /// </summary>
    public string AnswerOptionUserInput { get; private set; }

    /// <summary>
    /// Gets the follow-up question text.
    /// </summary>
    public string FollowUpQuestionText { get; private set; }

    /// <summary>
    /// Gets the follow-up answer user input.
    /// </summary>
    public string FollowUpAnswerUserInput { get; private set; }

    /// <summary>
    /// Gets the score.
    /// </summary>
    public int Score { get; private set; }

    /// <summary>
    /// Gets the FODA type.
    /// </summary>
    public FodaType Foda { get; private set; }

    /// <summary>
    /// Gets the FODA explanation.
    /// </summary>
    public string FodaExplanation { get; private set; }

    /// <summary>
    /// Gets the ODSR type.
    /// </summary>
    public OdsrType Odsr { get; private set; }

    /// <summary>
    /// Gets the ODSR explanation.
    /// </summary>
    public string OdsrExplanation { get; private set; }

    /// <summary>
    /// Gets the question phase.
    /// </summary>
    public QuestionPhase Phase { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this answer is used for mentoring plan.
    /// </summary>
    public bool IsUsedForMentoringPlan { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this answer is used for diagnosis.
    /// </summary>
    public bool IsUsedForDiagnosis { get; private set; }

    /// <summary>
    /// Gets the order within the phase.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Gets the submission date.
    /// </summary>
    public DateTime SubmittedAt { get; private set; }

    /// <summary>
    /// Gets the answer source (Starter or Coordinator).
    /// </summary>
    public string AnswerSource { get; private set; } = "Starter";

    /// <summary>
    /// Gets the coordinator user ID when source is Coordinator.
    /// </summary>
    public string? CoordinatorUserId { get; private set; }

    /// <summary>
    /// Gets whether this answer is preferred for diagnosis.
    /// </summary>
    public bool PreferredForDiagnosis { get; private set; }

    /// <summary>
    /// Creates a diagnosis answer from a form submission.
    /// </summary>
    /// <param name="input">The answer input data.</param>
    /// <param name="phase">The question phase.</param>
    /// <param name="order">The order within the phase.</param>
    /// <param name="submittedAt">The submission date.</param>
    /// <returns>A new DiagnosisAnswer instance.</returns>
    internal static DiagnosisAnswer CreateFromSubmission(
        DiagnosisAnswerInput input,
        QuestionPhase phase,
        int order,
        DateTime submittedAt)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (order <= 0)
        {
            throw new ArgumentException("Order must be positive", nameof(order));
        }

        return new DiagnosisAnswer(input, phase, order, submittedAt);
    }

    /// <summary>
    /// Updates the answer data.
    /// </summary>
    /// <param name="answerTopicId">The topic identifier.</param>
    /// <param name="answerTopicName">The topic name.</param>
    /// <param name="answerModuleId">The module identifier.</param>
    /// <param name="answerModuleName">The module name.</param>
    /// <param name="answerBlockId">The block identifier.</param>
    /// <param name="answerBlockName">The block name.</param>
    /// <param name="answerQuestionText">The question text.</param>
    /// <param name="answerAnswerOptionText">The answer option text.</param>
    /// <param name="answerAnswerOptionUserInput">The answer option user input.</param>
    /// <param name="answerFollowUpQuestionText">The follow-up question text.</param>
    /// <param name="answerFollowUpAnswerUserInput">The follow-up answer user input.</param>
    /// <param name="answerScore">The score.</param>
    /// <param name="parse">The FODA type.</param>
    /// <param name="answerFodaExplanation">The FODA explanation.</param>
    /// <param name="odsrType">The ODSR type.</param>
    /// <param name="answerOdsrExplanation">The ODSR explanation.</param>
    /// <param name="answerPhase">The question phase.</param>
    /// <param name="answerIsUsedForMentoringPlan">Whether used for mentoring plan.</param>
    /// <param name="answerIsUsedForDiagnosis">Whether used for diagnosis.</param>
    internal void Update(
        long? answerTopicId,
        string answerTopicName,
        long? answerModuleId,
        string answerModuleName,
        long answerBlockId,
        string answerBlockName,
        string answerQuestionText,
        string answerAnswerOptionText,
        string answerAnswerOptionUserInput,
        string answerFollowUpQuestionText,
        string answerFollowUpAnswerUserInput,
        int answerScore,
        FodaType parse,
        string answerFodaExplanation,
        OdsrType odsrType,
        string answerOdsrExplanation,
        QuestionPhase answerPhase,
        bool answerIsUsedForMentoringPlan,
        bool answerIsUsedForDiagnosis)
    {
        TopicId = answerTopicId;
        TopicName = answerTopicName;
        ModuleId = answerModuleId;
        ModuleName = answerModuleName;
        BlockId = answerBlockId;
        BlockName = answerBlockName;
        QuestionText = answerQuestionText;
        AnswerOptionText = answerAnswerOptionText;
        AnswerOptionUserInput = answerAnswerOptionUserInput;
        FollowUpQuestionText = answerFollowUpQuestionText;
        FollowUpAnswerUserInput = answerFollowUpAnswerUserInput;
        Score = answerScore;
        Foda = parse;
        FodaExplanation = answerFodaExplanation;
        Odsr = odsrType;
        OdsrExplanation = answerOdsrExplanation;
        Phase = answerPhase;
        IsUsedForMentoringPlan = answerIsUsedForMentoringPlan;
        IsUsedForDiagnosis = answerIsUsedForDiagnosis;
    }
}
