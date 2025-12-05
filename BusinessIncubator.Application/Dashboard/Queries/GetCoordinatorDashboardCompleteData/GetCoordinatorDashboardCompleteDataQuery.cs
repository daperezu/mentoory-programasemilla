using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorDashboardCompleteData;

/// <summary>
/// Query to get complete dashboard data for coordinator in a single request.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetCoordinatorDashboardCompleteDataQuery(
    long ProjectId,
    string CoordinatorUserId,
    DateTime? DateRangeStart = null) : IBaseRequest<CoordinatorDashboardCompleteDto>;

/// <summary>
/// Complete dashboard data DTO for coordinator.
/// </summary>
public class CoordinatorDashboardCompleteDto
{
    /// <summary>
    /// Gets or sets the project context.
    /// </summary>
    public ProjectContextDto ProjectContext { get; set; } = new();

    /// <summary>
    /// Gets or sets the participant statistics.
    /// </summary>
    public ParticipantStatsDto ParticipantStats { get; set; } = new();

    /// <summary>
    /// Gets or sets the diagnostic statistics.
    /// </summary>
    public DiagnosticStatsDto DiagnosticStats { get; set; } = new();

    /// <summary>
    /// Gets or sets the pending reviews.
    /// </summary>
    public PendingReviewsDto PendingReviews { get; set; } = new();

    /// <summary>
    /// Gets or sets the recent activities.
    /// </summary>
    public List<ActivityItemDto> RecentActivities { get; set; } = [];

    /// <summary>
    /// Gets or sets the user lookup dictionary for avoiding N+1.
    /// </summary>
    public Dictionary<string, string> UserNames { get; set; } = [];
}

/// <summary>
/// Project context DTO.
/// </summary>
public class ProjectContextDto
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project key.
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator ID.
    /// </summary>
    public long IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;
}

/// <summary>
/// Participant statistics DTO.
/// </summary>
public class ParticipantStatsDto
{
    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the active count.
    /// </summary>
    public int ActiveCount { get; set; }

    /// <summary>
    /// Gets or sets the pending invitations count.
    /// </summary>
    public int PendingInvitations { get; set; }

    /// <summary>
    /// Gets or sets the recently added count.
    /// </summary>
    public int RecentlyAdded { get; set; }

    /// <summary>
    /// Gets or sets the count by role.
    /// </summary>
    public Dictionary<string, int> CountByRole { get; set; } = [];
}

/// <summary>
/// Diagnostic statistics DTO.
/// </summary>
public class DiagnosticStatsDto
{
    /// <summary>
    /// Gets or sets the total forms.
    /// </summary>
    public int TotalForms { get; set; }

    /// <summary>
    /// Gets or sets the completed forms.
    /// </summary>
    public int CompletedForms { get; set; }

    /// <summary>
    /// Gets or sets the in-progress forms.
    /// </summary>
    public int InProgressForms { get; set; }

    /// <summary>
    /// Gets or sets the not started count.
    /// </summary>
    public int NotStartedCount { get; set; }

    /// <summary>
    /// Gets or sets the completion rate.
    /// </summary>
    public double CompletionRate { get; set; }

    /// <summary>
    /// Gets or sets the average completion time in hours.
    /// </summary>
    public double AverageCompletionTimeHours { get; set; }
}

/// <summary>
/// Pending reviews DTO.
/// </summary>
public class PendingReviewsDto
{
    /// <summary>
    /// Gets or sets the total pending.
    /// </summary>
    public int TotalPending { get; set; }

    /// <summary>
    /// Gets or sets the top reviews.
    /// </summary>
    public List<PendingReviewItemDto> TopReviews { get; set; } = [];

    /// <summary>
    /// Gets or sets the oldest waiting days.
    /// </summary>
    public int OldestWaitingDays { get; set; }

    /// <summary>
    /// Gets or sets the average waiting days.
    /// </summary>
    public double AverageWaitingDays { get; set; }
}

/// <summary>
/// Pending review item DTO.
/// </summary>
public class PendingReviewItemDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the participant user ID.
    /// </summary>
    public string ParticipantUserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant name.
    /// </summary>
    public string ParticipantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submitted date.
    /// </summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the days waiting.
    /// </summary>
    public int DaysWaiting { get; set; }

    /// <summary>
    /// Gets or sets the form type.
    /// </summary>
    public string FormType { get; set; } = "Diagnóstico";
}

/// <summary>
/// Activity item DTO.
/// </summary>
public class ActivityItemDto
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action description.
    /// </summary>
    public string ActionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the time ago string.
    /// </summary>
    public string TimeAgo { get; set; } = string.Empty;
}
