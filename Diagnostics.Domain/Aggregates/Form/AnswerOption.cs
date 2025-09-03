using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Aggregates.Form;

public class AnswerOption : Entity
{
    public AnswerOption(string text, int score, FodaType foda, string fodaExplanation, OdsrType odsr, string odsrExplanation, string? followUpQuestionText, int order)
    {
        Text = text;
        Score = score;
        Foda = foda;
        FodaExplanation = fodaExplanation;
        Odsr = odsr;
        OdsrExplanation = odsrExplanation;
        FollowUpQuestionText = followUpQuestionText;
        Order = order;
    }

    protected AnswerOption()
    {
    }

    public long QuestionId { get; private set; }

    public string Text { get; private set; }

    public int Score { get; private set; }

    public FodaType Foda { get; private set; }

    public string FodaExplanation { get; private set; }

    public OdsrType Odsr { get; private set; }

    public string OdsrExplanation { get; private set; }

    public int Order { get; private set; }

    public string? FollowUpQuestionText { get; private set; }

    public virtual Question Question { get; private set; }

    public AnswerOption UpdateText(string text)
    {
        Text = text;
        return this;
    }

    public AnswerOption UpdateScore(int score)
    {
        Score = score;
        return this;
    }

    public AnswerOption UpdateFoda(FodaType foda, string fodaExplanation)
    {
        Foda = foda;
        FodaExplanation = fodaExplanation;
        return this;
    }

    public AnswerOption UpdateOdsr(OdsrType odsr, string odsrExplanation)
    {
        Odsr = odsr;
        OdsrExplanation = odsrExplanation;
        return this;
    }

    public AnswerOption UpdateFollowUpQuestionText(string? followUpQuestionText)
    {
        FollowUpQuestionText = followUpQuestionText;
        return this;
    }

    public AnswerOption ReOrder(int order)
    {
        Order = order;
        return this;
    }

    public void Update(string text, int score, FodaType foda, string fodaExplanation, OdsrType odsr, string odsrExplanation, string? followUpQuestionText, int order)
    {
        Text = text;
        Score = score;
        Foda = foda;
        FodaExplanation = fodaExplanation;
        Odsr = odsr;
        OdsrExplanation = odsrExplanation;
        FollowUpQuestionText = followUpQuestionText;
        Order = order;
    }

    internal void SetQuestionId(long questionId)
    {
        QuestionId = questionId;
    }
}
