using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class ProjectTopic : Entity
{
    private readonly List<ProjectQuestion> _projectQuestions = [];
    private readonly List<ProjectSubject> _projectSubjects = [];

    public ProjectTopic(
        long projectModuleId,
        long? sourceTopicId,
        string name,
        bool isNameCustomized,
        int order,
        bool isOrderCustomized)
    {
        ProjectModuleId = projectModuleId;
        SourceTopicId = sourceTopicId;
        Name = name;
        IsNameCustomized = isNameCustomized;
        Order = order;
        IsOrderCustomized = isOrderCustomized;
    }

    protected ProjectTopic()
    {
    }

    public long ProjectModuleId { get; private set; }

    public long? SourceTopicId { get; private set; }

    public string Name { get; private set; }

    public bool IsNameCustomized { get; private set; }

    public int Order { get; private set; }

    public bool IsOrderCustomized { get; private set; }

    public IReadOnlyCollection<ProjectQuestion> ProjectQuestions => _projectQuestions.AsReadOnly();

    public IReadOnlyCollection<ProjectSubject> ProjectSubjects => _projectSubjects.AsReadOnly();

    // Navigation property for EF Core
    internal virtual ProjectModule ProjectModule { get; private set; }

    public void UpdateName(string name, bool isCustomized)
    {
        Name = name;
        IsNameCustomized = isCustomized;
    }

    public void UpdateOrder(int order, bool isOrderCustomized)
    {
        Order = order;
        IsOrderCustomized = isOrderCustomized;
    }

    public ProjectQuestion AddProjectQuestion(
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
        var projectQuestion = new ProjectQuestion(
            Id,
            projectBlockId,
            sourceQuestionId,
            text,
            isTextCustomized,
            answerType,
            isAnswerTypeCustomized,
            appliesToPhase,
            isAppliesToPhaseCustomized,
            isUsedForMentoringPlan,
            isMentoringPlanCustomized,
            isUsedForDiagnosis,
            isDiagnosisCustomized,
            order,
            isOrderCustomized);

        _projectQuestions.Add(projectQuestion);

        return projectQuestion;
    }

    public ProjectSubject AddProjectSubject(
        long? sourceSubjectId,
        string title,
        bool isTitleCustomized,
        string? content,
        bool isContentCustomized,
        int order,
        bool isOrderCustomized)
    {
        var projectSubject = new ProjectSubject(
            Id,
            sourceSubjectId,
            title,
            isTitleCustomized,
            content,
            isContentCustomized,
            order,
            isOrderCustomized);
        _projectSubjects.Add(projectSubject);
        return projectSubject;
    }

    /// <summary>
    /// Checks if a question with the given source ID exists in this topic.
    /// </summary>
    /// <param name="sourceQuestionId">The source question ID to check.</param>
    /// <returns>True if exists, false otherwise.</returns>
    public bool HasQuestionWithSourceId(long? sourceQuestionId)
    {
        return sourceQuestionId.HasValue && _projectQuestions.Any(q => q.SourceQuestionId == sourceQuestionId.Value);
    }

    /// <summary>
    /// Checks if a question with the given text and block exists in this topic.
    /// </summary>
    /// <param name="text">The question text.</param>
    /// <param name="blockId">The block ID.</param>
    /// <returns>True if exists, false otherwise.</returns>
    public bool HasQuestionWithTextAndBlock(string text, long blockId)
    {
        return _projectQuestions.Any(q => q.Text == text && q.ProjectBlockId == blockId);
    }

    public void ClearSource()
    {
        SourceTopicId = null;
    }

    public void CustomizeName(string name)
    {
        UpdateName(name, isCustomized: true);
    }

    public void ResetNameToSource(string sourceName)
    {
        UpdateName(sourceName, isCustomized: false);
    }

    public void CustomizeOrder(int order)
    {
        UpdateOrder(order, isOrderCustomized: true);
    }

    public void ResetOrderToSource(int sourceOrder)
    {
        UpdateOrder(sourceOrder, isOrderCustomized: false);
    }

    public bool IsFullyCustomized() =>
        IsNameCustomized && IsOrderCustomized;

    public void RemoveProjectSubject(long subjectId)
    {
        var subject = _projectSubjects.FirstOrDefault(s => s.Id == subjectId);
        if (subject is not null)
        {
            _projectSubjects.Remove(subject);
        }
    }
}
