namespace LinaSys.Web.Models.EntrepreneurForm;

public class FormBlockViewModel
{
    public long BlockId { get; set; }
    public string BlockName { get; set; } = string.Empty;
    public string? BlockDescription { get; set; }
    public int Order { get; set; }
    public List<QuestionViewModel> Questions { get; set; } = [];
    public bool IsCompleted { get; set; }
    public int QuestionsAnswered { get; set; }
    public int TotalQuestions => Questions.Count;

    public string IconClass => IsCompleted
        ? "fa-check-circle text-success"
        : "fa-circle text-muted";
}
