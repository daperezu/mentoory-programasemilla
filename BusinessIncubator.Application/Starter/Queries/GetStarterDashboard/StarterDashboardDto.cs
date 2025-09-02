namespace LinaSys.BusinessIncubator.Application.Starter.Queries.GetStarterDashboard;

public class StarterDashboardDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public StarterProgressDto Progress { get; set; } = new();
    public List<StarterTaskDto> Tasks { get; set; } = [];
    public List<StarterTaskDto> OverdueTasks { get; set; } = [];
    public List<StarterTaskDto> UpcomingTasks { get; set; } = [];
    public MentorInfoDto? MentorInfo { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = [];
    public List<MilestoneDto> Milestones { get; set; } = [];
    public List<FormStatusDto> FormStatuses { get; set; } = [];
    public StarterMetricsDto Metrics { get; set; } = new();
}

public class StarterProgressDto
{
    public string CurrentPhase { get; set; } = string.Empty;
    public DateTime PhaseStartDate { get; set; }
    public DateTime? PhaseExpectedEndDate { get; set; }
    public decimal OverallProgress { get; set; }
    public decimal PhaseProgress { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksTotal { get; set; }
    public int TasksOverdue { get; set; }
    public int FormsCompleted { get; set; }
    public int FormsTotal { get; set; }
    public int FormsRejected { get; set; }
    public int MilestonesAchieved { get; set; }
    public int MilestonesTotal { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? NextMilestoneDate { get; set; }
    public string? NextMilestoneName { get; set; }
    public decimal? EngagementScore { get; set; }
    public decimal? PerformanceScore { get; set; }
    public int DaysSinceStart { get; set; }
}

public class StarterTaskDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? AssignedBy { get; set; }
    public string? Category { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysUntilDue { get; set; }
    public string? ActionUrl { get; set; }
}

public class MentorInfoDto
{
    public string MentorId { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public string MentorEmail { get; set; } = string.Empty;
    public string? MentorPhone { get; set; }
    public string? MentorPhoto { get; set; }
    public DateTime? NextMeetingDate { get; set; }
    public string? MeetingUrl { get; set; }
    public int SessionsCompleted { get; set; }
    public int SessionsTotal { get; set; }
}

public class RecentActivityDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
}

public class MilestoneDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Progress { get; set; }
    public List<string> Requirements { get; set; } = [];
}

public class FormStatusDto
{
    public long FormId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewerComments { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
}

public class StarterMetricsDto
{
    public decimal OverallProgress { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
    public int UnreadNotifications { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int DaysSinceStart { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public int FormsCompleted { get; set; }
    public int FormsTotal { get; set; }
    public int OverdueTasks { get; set; }
    public decimal CompletionRate { get; set; }
    public string ActivityStatus { get; set; } = string.Empty;
}