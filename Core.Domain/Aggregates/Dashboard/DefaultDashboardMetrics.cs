namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// Default implementation of dashboard metrics for general use.
/// </summary>
public class DefaultDashboardMetrics : DashboardMetrics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDashboardMetrics"/> class.
    /// </summary>
    /// <param name="overallProgress">The overall progress percentage.</param>
    /// <param name="pendingItems">The number of pending items.</param>
    /// <param name="completedItems">The number of completed items.</param>
    /// <param name="totalItems">The total number of items.</param>
    /// <param name="unreadNotifications">The number of unread notifications.</param>
    /// <param name="lastActivityDate">The last activity date.</param>
    /// <param name="daysSinceStart">The number of days since start.</param>
    /// <param name="currentPhase">The current phase or status.</param>
    public DefaultDashboardMetrics(
        decimal overallProgress,
        int pendingItems,
        int completedItems,
        int totalItems,
        int unreadNotifications,
        DateTime? lastActivityDate,
        int daysSinceStart,
        string currentPhase)
        : base(
            overallProgress,
            pendingItems,
            completedItems,
            totalItems,
            unreadNotifications,
            lastActivityDate,
            daysSinceStart,
            currentPhase)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDashboardMetrics"/> class with default values.
    /// </summary>
    public DefaultDashboardMetrics()
        : base(0, 0, 0, 0, 0, null, 0, "active")
    {
    }
}