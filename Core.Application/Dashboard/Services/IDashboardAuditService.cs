namespace LinaSys.Core.Application.Dashboard.Services;

/// <summary>
/// Service for auditing dashboard actions.
/// </summary>
public interface IDashboardAuditService
{
    /// <summary>
    /// Logs a dashboard access event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogDashboardAccessAsync(string userId, long projectId, string dashboardType);

    /// <summary>
    /// Logs a widget interaction event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogWidgetInteractionAsync(string userId, long widgetId, string action, string? details = null);

    /// <summary>
    /// Logs a task completion event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogTaskCompletionAsync(string userId, long taskId, string? notes = null);

    /// <summary>
    /// Logs a preference update event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogPreferenceUpdateAsync(string userId, string preferenceType, string oldValue, string newValue);

    /// <summary>
    /// Logs a notification interaction event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogNotificationInteractionAsync(string userId, long notificationId, string action);

    /// <summary>
    /// Logs a security event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogSecurityEventAsync(string userId, string eventType, string details, bool isSuccessful);

    /// <summary>
    /// Logs a performance metric.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogPerformanceMetricAsync(string userId, string action, long elapsedMilliseconds, string? details = null);
}