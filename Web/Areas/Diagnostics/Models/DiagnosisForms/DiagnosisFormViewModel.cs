namespace LinaSys.Web.Areas.Diagnostics.Models.DiagnosisForms;

public enum AnswerTypeViewModel
{
    SingleChoice = 1,
    MultiChoice = 2,
    FreeText = 3,
    Numeric = 4,
    Date = 5,
    PersonId = 6,
    IdType = 7,
    Gender = 8,
    MaritalStatus = 9,
    Email = 10,
    PhoneNumber = 11,
    Nationality = 12,
}

public sealed record DiagnosisFormViewModel
{
    public List<BlockViewModel> QuestionBlocks { get; set; } = [];
}

public sealed record BlockViewModel(long Id, string Title, List<QuestionViewModel> Questions);

public sealed record QuestionViewModel(long Id, AnswerTypeViewModel AnswerType, string Text, bool Overriden, bool IsRequired, List<AnswerOptionViewModel> Options);

public sealed record AnswerOptionViewModel(long Id, string Text, bool Overriden, string? FollowUpQuestionText, bool IsRequired);
