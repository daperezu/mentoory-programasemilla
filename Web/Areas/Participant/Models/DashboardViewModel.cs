namespace LinaSys.Web.Areas.Participant.Models;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public List<ProjectCardViewModel> Projects { get; set; } = new();
    public List<PendingFormViewModel> PendingForms { get; set; } = new();
    public List<ActivityViewModel> RecentActivities { get; set; } = new();
    public List<ConvocationViewModel> OpenConvocations { get; set; } = new();

    // Statistics
    public int ActiveProjectsCount => Projects.Count(p => p.IsActive);
    public int PendingFormsCount => PendingForms.Count;
    public int UnreadNotificationsCount { get; set; }
}

public class ProjectCardViewModel
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = "primary";
    public string IncubatorName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Progress { get; set; }
    public bool IsActive { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "/images/project-placeholder.png";
}

public class PendingFormViewModel
{
    public long FormId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysRemaining => (DueDate.Date - DateTime.UtcNow.Date).Days;
    public string Urgency
    {
        get
        {
            if (DaysRemaining <= 0)
            {
                return "danger";
            }

            if (DaysRemaining <= 3)
            {
                return "warning";
            }

            return "info";
        }
    }

    public string FormType { get; set; } = string.Empty;
    public string FormUrl { get; set; } = string.Empty;
}

public class ActivityViewModel
{
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-info-circle";
    public string IconColor { get; set; } = "primary";
    public string RelatedEntity { get; set; } = string.Empty;
    public string RelatedEntityUrl { get; set; } = string.Empty;
}

public class ConvocationViewModel
{
    public long ConvocationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OpenDate { get; set; }
    public DateTime CloseDate { get; set; }
    public int DaysRemaining => (CloseDate.Date - DateTime.UtcNow.Date).Days;
    public bool IsOpen => DateTime.UtcNow >= OpenDate && DateTime.UtcNow <= CloseDate;
    public string IncubatorName { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string Benefits { get; set; } = string.Empty;
    public string ApplyUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "/images/convocation-placeholder.png";
}
