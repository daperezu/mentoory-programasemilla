using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.Web.Areas.Participant.Models;

public class ProjectDashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string SelectedProjectName { get; set; } = string.Empty;
    public ProjectDetailsViewModel Project { get; set; } = new();
    public List<AvailableFormViewModel> AvailableForms { get; set; } = new();
    public List<ProjectActivityViewModel> RecentActivities { get; set; } = new();

    // Statistics for selected project only
    public int PendingFormsCount => AvailableForms.Count(f => f.Status == ProjectFormSubmissionStatus.Draft);
    public int AvailableFormsCount => AvailableForms.Count(f => !f.IsCreated);
    public int CompletedFormsCount => AvailableForms.Count(f => f.Status == ProjectFormSubmissionStatus.Approved);
}

public class ProjectDetailsViewModel
{
    public long ProjectId { get; set; }
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public DateTime? StageEndDate { get; set; }
    public int Progress { get; set; }
    public string IncubatorName { get; set; } = string.Empty;
    public Guid? IncubatorExternalId { get; set; }
    public string MentorName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class AvailableFormViewModel
{
    public Guid? ExistingFormId { get; set; } // External ID if form is created
    public string FormName { get; set; } = string.Empty;
    public QuestionPhase Phase { get; set; }
    public string StageName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsCreated { get; set; }
    public ProjectFormSubmissionStatus Status { get; set; }
    public double CompletionPercentage { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string ActionClass { get; set; } = string.Empty;
    public int PendingFeedbackCount { get; set; } // Number of feedback items requiring response
    public bool HasPendingFeedback => PendingFeedbackCount > 0;
}

public class ProjectActivityViewModel
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
}