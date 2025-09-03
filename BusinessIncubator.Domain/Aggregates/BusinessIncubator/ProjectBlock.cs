using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class ProjectBlock : Entity
{
    private readonly List<ProjectQuestion> _projectQuestions = [];

    public ProjectBlock(long projectId, long? sourceBlockId, string name, bool isNameCustomized)
    {
        ProjectId = projectId;
        SourceBlockId = sourceBlockId;
        Name = name;
        IsNameCustomized = isNameCustomized;
    }

    protected ProjectBlock()
    {
    }

    public long? SourceBlockId { get; private set; }

    public long ProjectId { get; private set; }

    public string Name { get; private set; }

    public bool IsNameCustomized { get; private set; }

    public IReadOnlyCollection<ProjectQuestion> ProjectQuestions => _projectQuestions.AsReadOnly();

    // Navigation property for EF Core
    internal virtual Project Project { get; private set; }

    public void UpdateName(string name, bool isCustomized)
    {
        Name = name;
        IsNameCustomized = isCustomized;
    }

    public void UpdateSourceBlockId(long? sourceBlockId)
    {
        SourceBlockId = sourceBlockId;
    }

    public void UpdateProjectId(long projectId)
    {
        ProjectId = projectId;
    }

    public ProjectQuestion AddProjectQuestion(
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
            projectTopicId: null, // Block-level question without topic
            projectBlockId: this.Id,
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

    public bool HasQuestionWithSourceId(long? sourceQuestionId)
    {
        return sourceQuestionId.HasValue &&
               _projectQuestions.Any(q => q.SourceQuestionId == sourceQuestionId.Value);
    }

    public bool HasQuestionWithText(string text)
    {
        return _projectQuestions.Any(q => q.Text == text);
    }

    public void RemoveProjectQuestion(long questionId)
    {
        var question = _projectQuestions.FirstOrDefault(q => q.Id == questionId);
        if (question is not null)
        {
            _projectQuestions.Remove(question);
        }
    }
}
