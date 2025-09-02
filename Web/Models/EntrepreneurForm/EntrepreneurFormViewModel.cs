using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.Web.Models.EntrepreneurForm;

public class EntrepreneurFormViewModel
{
    public Guid SubmissionExternalId { get; set; }
    public Guid ProjectExternalId { get; set; } // Kept for compatibility but not primary identifier
    public string ProjectName { get; set; } = string.Empty;
    public long FormSubmissionId { get; set; }
    public LinaSys.BusinessIncubator.Domain.Enums.QuestionPhase Phase { get; set; }
    public List<FormBlockViewModel> Blocks { get; set; } = [];
    public int CurrentProgress { get; set; }
    public DateTime? DueDate { get; set; }
    public ProjectFormSubmissionStatus Status { get; set; }
    public string? DraftData { get; set; }

    // Display helpers
    public string PhaseDisplay => Phase == LinaSys.BusinessIncubator.Domain.Enums.QuestionPhase.Start
        ? "Formulario Inicial"
        : "Formulario Final";

    public string StatusDisplay => Status switch
    {
        ProjectFormSubmissionStatus.Draft => "Borrador",
        ProjectFormSubmissionStatus.Submitted => "Enviado",
        ProjectFormSubmissionStatus.Approved => "Aprobado",
        ProjectFormSubmissionStatus.Rejected => "Rechazado",
        _ => "Desconocido"
    };

    public bool CanEdit => Status == ProjectFormSubmissionStatus.Draft;

    public int DaysRemaining => DueDate.HasValue
        ? Math.Max(0, (int)(DueDate.Value - DateTime.Now).TotalDays)
        : 0;
}
