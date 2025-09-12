namespace LinaSys.BusinessIncubator.Domain.DTOs;

/// <summary>
/// Optimized DTO for dashboard project data fetched in a single query.
/// </summary>
public class DashboardProjectData
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

    /// <summary>
    /// Gets or sets the total number of users.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of active users.
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of recent users.
    /// </summary>
    public int RecentUsers { get; set; }

    /// <summary>
    /// Gets or sets the users count by role.
    /// </summary>
    public Dictionary<string, int> UsersByRole { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of forms.
    /// </summary>
    public int TotalForms { get; set; }

    /// <summary>
    /// Gets or sets the number of completed forms.
    /// </summary>
    public int CompletedForms { get; set; }

    /// <summary>
    /// Gets or sets the number of in-progress forms.
    /// </summary>
    public int InProgressForms { get; set; }

    /// <summary>
    /// Gets or sets the number of not started forms.
    /// </summary>
    public int NotStartedForms { get; set; }

    /// <summary>
    /// Gets or sets the average completion time in hours.
    /// </summary>
    public double AverageCompletionHours { get; set; }

    /// <summary>
    /// Gets or sets the total pending reviews.
    /// </summary>
    public int TotalPendingReviews { get; set; }

    /// <summary>
    /// Gets or sets the pending invitations count.
    /// </summary>
    public int PendingInvitations { get; set; }

    /// <summary>
    /// Gets or sets the pending reviews.
    /// </summary>
    public List<PendingReviewData> PendingReviews { get; set; } = [];

    /// <summary>
    /// Gets or sets the recent activities.
    /// </summary>
    public List<ActivityData> RecentActivities { get; set; } = [];

    /// <summary>
    /// Gets or sets all user IDs for batch loading.
    /// </summary>
    public List<string> AllUserIds { get; set; } = [];
}

/// <summary>
/// Pending review data.
/// </summary>
public class PendingReviewData
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submitted at date.
    /// </summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the days waiting.
    /// </summary>
    public int DaysWaiting { get; set; }
}

/// <summary>
/// Activity data.
/// </summary>
public class ActivityData
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}