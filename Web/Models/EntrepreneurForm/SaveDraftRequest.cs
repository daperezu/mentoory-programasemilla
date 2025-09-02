namespace LinaSys.Web.Models.EntrepreneurForm;

public class SaveDraftRequest
{
    public long SubmissionId { get; set; }
    public string DraftData { get; set; } = string.Empty;
    public int AnsweredQuestions { get; set; }
    public int TotalQuestions { get; set; }
}

public class SubmitFormRequest
{
    public long SubmissionId { get; set; }
}
