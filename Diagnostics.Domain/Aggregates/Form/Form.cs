using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Aggregates.Form;

public class Form : Entity, IAggregateRoot
{
    private readonly List<FormQuestion> _formQuestions = [];

    public Form(string name)
    {
        Name = name;
    }

    protected Form()
    {
    }

    public string Name { get; private set; }

    public long? SourceKnowledgeStructureId { get; private set; }

    public IReadOnlyCollection<FormQuestion> FormQuestions => _formQuestions.AsReadOnly();

    public FormQuestion AddQuestion(long? topicId, long blockId, string text, AnswerType answerType, QuestionPhase appliesToPhase, bool isUsedForMentoringPlan, bool isUsedForDiagnosis, int order)
    {
        var question = new Question(text, answerType, appliesToPhase, isUsedForMentoringPlan, isUsedForDiagnosis);
        return AddQuestion(question, topicId, blockId, order);
    }

    public FormQuestion AddQuestion(Question question, long? topicId, long blockId, int order)
    {
        if (question is null)
        {
            throw new ArgumentNullException(nameof(question), "Question cannot be null.");
        }

        if (question.Id > 0 && _formQuestions.Any(fq => fq.QuestionId == question.Id))
        {
            throw new InvalidOperationException("This question is already added to the form.");
        }

        var form = new FormQuestion(this, question, topicId, blockId, order);
        _formQuestions.Add(form);
        return form;
    }

    public void RemoveQuestion(long questionId)
    {
        var formQuestion = _formQuestions.FirstOrDefault(fq => fq.QuestionId == questionId);
        if (formQuestion is null)
        {
            throw new InvalidOperationException("This question is not part of the form.");
        }

        _formQuestions.Remove(formQuestion);
    }

    public void Rename(string name)
    {
        Name = name;
    }

    public Form SetSourceKnowledgeStructure(long knowledgeStructureId)
    {
        SourceKnowledgeStructureId = knowledgeStructureId;
        return this;
    }
}
