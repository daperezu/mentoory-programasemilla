namespace LinaSys.Web.Areas.Diagnostics.Models.DiagnosisForms;

public sealed record AnswerViewModel(long QuestionId, long? AnswerOptionId, string? UserInput, string? FollowUpUserInput);
