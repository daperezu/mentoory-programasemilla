namespace LinaSys.Web.Models.EntrepreneurForm;

/// <summary>
/// View model for the form submission success page.
/// </summary>
public class FormSuccessViewModel
{
    public Guid SubmissionExternalId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }

    // Display helpers
    public string FormattedSubmittedAt => SubmittedAt.ToString("dd/MM/yyyy HH:mm");
    public string StatusDisplay => Status switch
    {
        "Submitted" => "Enviado exitosamente",
        "Approved" => "Aprobado",
        _ => Status
    };
}
