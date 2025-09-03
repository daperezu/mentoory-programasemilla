using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class ProjectQuestion : Entity
{
    public ProjectQuestion(
        long? projectTopicId,
        long projectBlockId,
        long? sourceQuestionId,
        string text,
        bool isTextCustomized,
        AnswerType answerType,
        bool isAnswerTypeCustomized,
        QuestionPhase appliesToPhase,
        bool isAppliesToPhaseCustomized,
        bool isUsedForMentoringPlan,
        bool isMentoringPlanCustomized,
        bool isUsedForDiagnosis,
        bool isDiagnosisCustomized,
        int order,
        bool isOrderCustomized)
    {
        ProjectTopicId = projectTopicId;
        ProjectBlockId = projectBlockId;
        SourceQuestionId = sourceQuestionId;
        Text = text;
        IsTextCustomized = isTextCustomized;
        AnswerType = answerType;
        IsAnswerTypeCustomized = isAnswerTypeCustomized;
        AppliesToPhase = appliesToPhase;
        IsAppliesToPhaseCustomized = isAppliesToPhaseCustomized;
        IsUsedForMentoringPlan = isUsedForMentoringPlan;
        IsMentoringPlanCustomized = isMentoringPlanCustomized;
        IsUsedForDiagnosis = isUsedForDiagnosis;
        IsDiagnosisCustomized = isDiagnosisCustomized;
        Order = order;
        IsOrderCustomized = isOrderCustomized;
    }

    protected ProjectQuestion()
    {
    }

    public long? ProjectTopicId { get; private set; }

    public long ProjectBlockId { get; private set; }

    public long? SourceQuestionId { get; private set; }

    public string Text { get; private set; }

    public bool IsTextCustomized { get; private set; }

    public AnswerType AnswerType { get; private set; }

    public bool IsAnswerTypeCustomized { get; private set; }

    public QuestionPhase AppliesToPhase { get; private set; }

    public bool IsAppliesToPhaseCustomized { get; private set; }

    public bool IsUsedForMentoringPlan { get; private set; }

    public bool IsMentoringPlanCustomized { get; private set; }

    public bool IsUsedForDiagnosis { get; private set; }

    public bool IsDiagnosisCustomized { get; private set; }

    public int Order { get; private set; }

    public bool IsOrderCustomized { get; private set; }

    public virtual ICollection<ProjectAnswerOption> ProjectAnswerOptions { get; private set; } = [];

    public virtual ProjectBlock ProjectBlock { get; private set; }

    public virtual ProjectTopic? ProjectTopic { get; private set; }

    public void UpdateText(string text, bool isCustomized)
    {
        Text = text;
        IsTextCustomized = isCustomized;
    }

    public void UpdateAnswerType(AnswerType answerType, bool isCustomized)
    {
        AnswerType = answerType;
        IsAnswerTypeCustomized = isCustomized;
    }

    public void UpdateAppliesToPhase(QuestionPhase appliesToPhase, bool isCustomized)
    {
        AppliesToPhase = appliesToPhase;
        IsAppliesToPhaseCustomized = isCustomized;
    }

    public void UpdateUsedForMentoringPlan(bool isUsed, bool isCustomized)
    {
        IsUsedForMentoringPlan = isUsed;
        IsMentoringPlanCustomized = isCustomized;
    }

    public void UpdateUsedForDiagnosis(bool isUsed, bool isCustomized)
    {
        IsUsedForDiagnosis = isUsed;
        IsDiagnosisCustomized = isCustomized;
    }

    public void UpdateOrder(int order, bool isCustomized)
    {
        Order = order;
        IsOrderCustomized = isCustomized;
    }

    /// <summary>
    /// Updates the topic assignment for this question.
    /// Note: When a topic is deleted, this method should be called with null
    /// to remove the topic reference, as the FK constraint uses NO ACTION.
    /// </summary>
    /// <param name="topicId">The new topic ID or null to remove the assignment.</param>
    public void UpdateTopicId(long? topicId)
    {
        ProjectTopicId = topicId;
    }

    public ProjectAnswerOption AddProjectAnswerOption(
        long? sourceAnswerOptionId,
        string text,
        bool isTextCustomized,
        int score,
        bool isScoreCustomized,
        FodaType foda,
        bool isFodaCustomized,
        string fodaExplanation,
        bool isFodaExplanationCustomized,
        OdsrType odsr,
        bool isOdsrCustomized,
        string odsrExplanation,
        bool isOdsrExplanationCustomized,
        int order,
        bool isOrderCustomized,
        string followUpQuestionText,
        bool isFollowUpTextCustomized)
    {
        var answerOption = new ProjectAnswerOption(
            this.Id,
            sourceAnswerOptionId,
            text,
            isTextCustomized,
            score,
            isScoreCustomized,
            foda,
            isFodaCustomized,
            fodaExplanation,
            isFodaExplanationCustomized,
            odsr,
            isOdsrCustomized,
            odsrExplanation,
            isOdsrExplanationCustomized,
            order,
            isOrderCustomized,
            followUpQuestionText,
            isFollowUpTextCustomized);

        ProjectAnswerOptions.Add(answerOption);

        return answerOption;
    }

    public void RemoveProjectAnswerOption(long answerOptionId)
    {
        var answerOption = ProjectAnswerOptions.FirstOrDefault(x => x.Id == answerOptionId);

        if (answerOption is not null)
        {
            ProjectAnswerOptions.Remove(answerOption);
        }
    }

    public void UpdateIsUsedForMentoringPlan(bool isUsedForMentoringPlan, bool isMentoringPlanCustomized)
    {
        IsUsedForMentoringPlan = isUsedForMentoringPlan;
        IsMentoringPlanCustomized = isMentoringPlanCustomized;
    }

    public void UpdateIsUsedForDiagnosis(bool isUsedForDiagnosis, bool isDiagnosisCustomized)
    {
        IsUsedForDiagnosis = isUsedForDiagnosis;
        IsDiagnosisCustomized = isDiagnosisCustomized;
    }

    public void ClearSource()
    {
        SourceQuestionId = null;
    }

    public void CustomizeText(string text)
    {
        UpdateText(text, isCustomized: true);
    }

    public void ResetTextToSource(string sourceText)
    {
        UpdateText(sourceText, isCustomized: false);
    }

    public void CustomizeAnswerType(AnswerType answerType)
    {
        UpdateAnswerType(answerType, isCustomized: true);
    }

    public void ResetAnswerTypeToSource(AnswerType sourceAnswerType)
    {
        UpdateAnswerType(sourceAnswerType, isCustomized: false);
    }

    public void CustomizeAppliesToPhase(QuestionPhase phase)
    {
        UpdateAppliesToPhase(phase, isCustomized: true);
    }

    public void ResetAppliesToPhaseToSource(QuestionPhase sourcePhase)
    {
        UpdateAppliesToPhase(sourcePhase, isCustomized: false);
    }

    public void CustomizeOrder(int order)
    {
        UpdateOrder(order, isCustomized: true);
    }

    public void ResetOrderToSource(int sourceOrder)
    {
        UpdateOrder(sourceOrder, isCustomized: false);
    }

    public bool IsFullyCustomized() =>
        IsTextCustomized &&
        IsAnswerTypeCustomized &&
        IsAppliesToPhaseCustomized &&
        IsMentoringPlanCustomized &&
        IsDiagnosisCustomized &&
        IsOrderCustomized;
}
