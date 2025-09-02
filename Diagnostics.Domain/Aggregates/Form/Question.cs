using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Aggregates.Form;

public class Question : Entity
{
    private readonly List<AnswerOption> _answerOptions = [];

    public Question(string text, AnswerType answerType, QuestionPhase appliesToPhase, bool isUsedForMentoringPlan, bool isUsedForDiagnosis)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Question text cannot be empty.", nameof(text));
        }

        if (text.Length > 500)
        {
            throw new ArgumentException("Question text cannot exceed 500 characters.", nameof(text));
        }

        Text = text;
        AnswerType = answerType;
        AppliesToPhase = appliesToPhase;
        IsUsedForMentoringPlan = isUsedForMentoringPlan;
        IsUsedForDiagnosis = isUsedForDiagnosis;
    }

    protected Question()
    {
    }

    public string Text { get; private set; }

    public AnswerType AnswerType { get; private set; }

    public QuestionPhase AppliesToPhase { get; private set; }

    public bool IsUsedForMentoringPlan { get; private set; }

    public bool IsUsedForDiagnosis { get; private set; }

    public IReadOnlyCollection<AnswerOption> AnswerOptions => _answerOptions.AsReadOnly();

    // Navigation property for EF Core - not part of the aggregate
    internal virtual ICollection<FormQuestion> FormQuestions { get; private set; } = [];

    public void AddAnswerOption(string text, int score, FodaType foda, string fodaExplanation, OdsrType odsr, string odsrExplanation, string? followupQuestionText, int order)
    {
        var answerOption = new AnswerOption(text, score, foda, fodaExplanation, odsr, odsrExplanation, followupQuestionText, order);
        answerOption.SetQuestionId(Id);
        AddAnswerOption(answerOption);
    }

    public void AddAnswerOption(AnswerOption answerOption)
    {
        if (answerOption is null)
        {
            throw new ArgumentNullException(nameof(answerOption), "Answer option cannot be null.");
        }

        if (answerOption.QuestionId != Id)
        {
            throw new InvalidOperationException("Answer option must belong to this question.");
        }

        _answerOptions.Add(answerOption);
    }

    public void Update(string text, AnswerType answerType, QuestionPhase appliesToPhase, bool isUsedForMentoringPlan, bool isUsedForDiagnosis)
    {
        Text = text;
        AnswerType = answerType;
        AppliesToPhase = appliesToPhase;
        IsUsedForMentoringPlan = isUsedForMentoringPlan;
        IsUsedForDiagnosis = isUsedForDiagnosis;
    }

    public void RemoveAnswerOptionById(long optionId)
    {
        var option = _answerOptions.FirstOrDefault(x => x.Id == optionId);
        if (option is not null)
        {
            _answerOptions.Remove(option);
        }
    }
}
