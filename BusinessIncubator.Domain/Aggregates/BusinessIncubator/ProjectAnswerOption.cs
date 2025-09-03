using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class ProjectAnswerOption : Entity
{
    public ProjectAnswerOption(
        long projectQuestionId,
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
        ProjectQuestionId = projectQuestionId;
        SourceAnswerOptionId = sourceAnswerOptionId;
        Text = text;
        IsTextCustomized = isTextCustomized;
        Score = score;
        IsScoreCustomized = isScoreCustomized;
        Foda = foda;
        IsFodaCustomized = isFodaCustomized;
        FodaExplanation = fodaExplanation;
        IsFodaExplanationCustomized = isFodaExplanationCustomized;
        Odsr = odsr;
        IsOdsrCustomized = isOdsrCustomized;
        OdsrExplanation = odsrExplanation;
        IsOdsrExplanationCustomized = isOdsrExplanationCustomized;
        Order = order;
        IsOrderCustomized = isOrderCustomized;
        FollowUpQuestionText = followUpQuestionText;
        IsFollowUpTextCustomized = isFollowUpTextCustomized;
    }

    protected ProjectAnswerOption()
    {
    }

    public long ProjectQuestionId { get; private set; }

    public long? SourceAnswerOptionId { get; private set; }

    public string Text { get; private set; }

    public bool IsTextCustomized { get; private set; }

    public int Score { get; private set; }

    public bool IsScoreCustomized { get; private set; }

    public FodaType Foda { get; private set; }

    public bool IsFodaCustomized { get; private set; }

    public string FodaExplanation { get; private set; }

    public bool IsFodaExplanationCustomized { get; private set; }

    public OdsrType Odsr { get; private set; }

    public bool IsOdsrCustomized { get; private set; }

    public string OdsrExplanation { get; private set; }

    public bool IsOdsrExplanationCustomized { get; private set; }

    public int Order { get; private set; }

    public bool IsOrderCustomized { get; private set; }

    public string FollowUpQuestionText { get; private set; }

    public bool IsFollowUpTextCustomized { get; private set; }

    public virtual ProjectQuestion ProjectQuestion { get; private set; }

    public virtual ICollection<ProjectSubject> ProjectSubjects { get; private set; } = [];

    public void UpdateText(string text, bool isCustomized)
    {
        Text = text;
        IsTextCustomized = isCustomized;
    }

    public void UpdateScore(int score, bool isCustomized)
    {
        Score = score;
        IsScoreCustomized = isCustomized;
    }

    public void UpdateFoda(FodaType foda, bool isCustomized)
    {
        Foda = foda;
        IsFodaCustomized = isCustomized;
    }

    public void UpdateFodaExplanation(string explanation, bool isCustomized)
    {
        FodaExplanation = explanation;
        IsFodaExplanationCustomized = isCustomized;
    }

    public void UpdateOdsr(OdsrType odsr, bool isCustomized)
    {
        Odsr = odsr;
        IsOdsrCustomized = isCustomized;
    }

    public void UpdateOdsrExplanation(string explanation, bool isCustomized)
    {
        OdsrExplanation = explanation;
        IsOdsrExplanationCustomized = isCustomized;
    }

    public void UpdateOrder(int order, bool isCustomized)
    {
        Order = order;
        IsOrderCustomized = isCustomized;
    }

    public void UpdateFollowUpQuestionText(string? text, bool isCustomized)
    {
        FollowUpQuestionText = text ?? string.Empty;
        IsFollowUpTextCustomized = isCustomized;
    }

    public ProjectSubject AddProjectSubject(
        long? sourceSubjectId,
        string title,
        bool isTitleCustomized,
        string content,
        bool isContentCustomized,
        int order,
        bool isOrderCustomized)
    {
        var projectSubject = new ProjectSubject(
            ProjectQuestionId,
            sourceSubjectId,
            title,
            isTitleCustomized,
            content,
            isContentCustomized,
            order,
            isOrderCustomized);

        ProjectSubjects.Add(projectSubject);

        return projectSubject;
    }

    public void RemoveProjectSubject(long projectSubjectId)
    {
        var projectSubject = ProjectSubjects.FirstOrDefault(ps => ps.Id == projectSubjectId);

        if (projectSubject is not null)
        {
            ProjectSubjects.Remove(projectSubject);
        }
    }

    public void ClearSource()
    {
        SourceAnswerOptionId = null;
    }

    public void CustomizeText(string text)
    {
        UpdateText(text, isCustomized: true);
    }

    public void ResetTextToSource(string sourceText)
    {
        UpdateText(sourceText, isCustomized: false);
    }

    public void CustomizeFollowUpQuestionText(string text)
    {
        UpdateFollowUpQuestionText(text, isCustomized: true);
    }

    public void ResetFollowUpQuestionTextToSource(string sourceText)
    {
        UpdateFollowUpQuestionText(sourceText, isCustomized: false);
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
        IsScoreCustomized &&
        IsFodaCustomized &&
        IsFodaExplanationCustomized &&
        IsOdsrCustomized &&
        IsOdsrExplanationCustomized &&
        IsOrderCustomized &&
        IsFollowUpTextCustomized;
}
