using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// Base class for dashboard metrics.
/// </summary>
public abstract class DashboardMetrics : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardMetrics"/> class.
    /// </summary>
    protected DashboardMetrics()
    {
        OverallProgress = 0;
        PendingItems = 0;
        CompletedItems = 0;
        TotalItems = 0;
        UnreadNotifications = 0;
        CurrentPhase = string.Empty;
        DaysSinceStart = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardMetrics"/> class.
    /// </summary>
    /// <param name="overallProgress">The overall progress percentage.</param>
    /// <param name="pendingItems">The number of pending items.</param>
    /// <param name="completedItems">The number of completed items.</param>
    /// <param name="totalItems">The total number of items.</param>
    /// <param name="unreadNotifications">The number of unread notifications.</param>
    /// <param name="lastActivityDate">The last activity date.</param>
    /// <param name="daysSinceStart">The number of days since start.</param>
    /// <param name="currentPhase">The current phase or status.</param>
    protected DashboardMetrics(
        decimal overallProgress,
        int pendingItems,
        int completedItems,
        int totalItems,
        int unreadNotifications,
        DateTime? lastActivityDate,
        int daysSinceStart,
        string currentPhase)
        : this()
    {
        OverallProgress = overallProgress >= 0 && overallProgress <= 100
            ? overallProgress
            : throw new ArgumentException("Overall progress must be between 0 and 100", nameof(overallProgress));
        PendingItems = pendingItems >= 0 ? pendingItems : throw new ArgumentException("Pending items cannot be negative", nameof(pendingItems));
        CompletedItems = completedItems >= 0 ? completedItems : throw new ArgumentException("Completed items cannot be negative", nameof(completedItems));
        TotalItems = totalItems >= 0 ? totalItems : throw new ArgumentException("Total items cannot be negative", nameof(totalItems));
        UnreadNotifications = unreadNotifications >= 0 ? unreadNotifications : throw new ArgumentException("Unread notifications cannot be negative", nameof(unreadNotifications));
        LastActivityDate = lastActivityDate;
        DaysSinceStart = daysSinceStart >= 0 ? daysSinceStart : throw new ArgumentException("Days since start cannot be negative", nameof(daysSinceStart));
        CurrentPhase = currentPhase ?? throw new ArgumentNullException(nameof(currentPhase));
    }

    /// <summary>
    /// Gets or sets the overall progress percentage.
    /// </summary>
    public decimal OverallProgress { get; protected set; }

    /// <summary>
    /// Gets or sets the total number of pending items.
    /// </summary>
    public int PendingItems { get; protected set; }

    /// <summary>
    /// Gets or sets the total number of completed items.
    /// </summary>
    public int CompletedItems { get; protected set; }

    /// <summary>
    /// Gets or sets the total number of items.
    /// </summary>
    public int TotalItems { get; protected set; }

    /// <summary>
    /// Gets or sets the number of unread notifications.
    /// </summary>
    public int UnreadNotifications { get; protected set; }

    /// <summary>
    /// Gets or sets the last activity date.
    /// </summary>
    public DateTime? LastActivityDate { get; protected set; }

    /// <summary>
    /// Gets or sets the days since start.
    /// </summary>
    public int DaysSinceStart { get; protected set; }

    /// <summary>
    /// Gets or sets the current phase or status.
    /// </summary>
    public string CurrentPhase { get; protected set; }

    /// <summary>
    /// Calculate completion rate.
    /// </summary>
    /// <returns>The completion rate as a percentage.</returns>
    public decimal GetCompletionRate()
    {
        if (TotalItems == 0)
        {
            return 0;
        }

        return Math.Round((decimal)CompletedItems / TotalItems * 100, 2);
    }

    /// <summary>
    /// Calculate pending rate.
    /// </summary>
    /// <returns>The pending rate as a percentage.</returns>
    public decimal GetPendingRate()
    {
        if (TotalItems == 0)
        {
            return 0;
        }

        return Math.Round((decimal)PendingItems / TotalItems * 100, 2);
    }

    /// <summary>
    /// Check if user is active.
    /// </summary>
    /// <param name="inactiveDaysThreshold">The number of days threshold for inactivity.</param>
    /// <returns>True if the user is active; otherwise, false.</returns>
    public bool IsActive(int inactiveDaysThreshold = 7)
    {
        if (!LastActivityDate.HasValue)
        {
            return false;
        }

        var daysSinceLastActivity = (DateTime.UtcNow - LastActivityDate.Value).Days;
        return daysSinceLastActivity <= inactiveDaysThreshold;
    }

    /// <summary>
    /// Get activity status.
    /// </summary>
    /// <returns>The activity status string.</returns>
    public string GetActivityStatus()
    {
        if (!LastActivityDate.HasValue)
        {
            return "Inactivo";
        }

        var daysSinceLastActivity = (DateTime.UtcNow - LastActivityDate.Value).Days;

        return daysSinceLastActivity switch
        {
            0 => "Activo hoy",
            1 => "Activo ayer",
            <= 7 => $"Activo hace {daysSinceLastActivity} días",
            <= 30 => "Activo este mes",
            _ => "Inactivo"
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OverallProgress;
        yield return PendingItems;
        yield return CompletedItems;
        yield return TotalItems;
        yield return UnreadNotifications;
        yield return LastActivityDate ?? DateTime.MinValue;
        yield return DaysSinceStart;
        yield return CurrentPhase;
    }
}