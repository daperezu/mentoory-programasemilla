namespace LinaSys.Web.Models.EntrepreneurForm;

public enum AnswerType
{
    Text = 0,
    Number = 1,
    Date = 2,
    SingleChoice = 3,
    MultiChoice = 4,
    Email = 5,
    TextArea = 6
}

public class QuestionViewModel
{
    public long QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? HelpText { get; set; }
    public AnswerType AnswerType { get; set; }
    public bool IsRequired { get; set; }
    public List<AnswerOptionViewModel> Options { get; set; } = [];
    public string? CurrentAnswer { get; set; }
    public ValidationRules? Validation { get; set; }

    public string InputType => AnswerType switch
    {
        AnswerType.SingleChoice => "radio",
        AnswerType.MultiChoice => "checkbox",
        AnswerType.Text => "text",
        AnswerType.Number => "number",
        AnswerType.Date => "date",
        AnswerType.Email => "email",
        AnswerType.TextArea => "textarea",
        _ => "text"
    };
}

public class AnswerOptionViewModel
{
    public long OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string? OptionValue { get; set; }
}

public class ValidationRules
{
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
}
