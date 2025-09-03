namespace LinaSys.Core.Application.Dashboard.Services;

/// <summary>
/// Service for auditing dashboard actions.
/// </summary>
public interface IDashboardAuditService
{
    /// <summary>
    /// Logs a dashboard access event.
    /// </summary>
    Task LogDashboardAccessAsync(string userId, long projectId, string dashboardType);

    /// <summary>
    /// Logs a widget interaction event.
    /// </summary>
    Task LogWidgetInteractionAsync(string userId, long widgetId, string action, string? details = null);

    /// <summary>
    /// Logs a task completion event.
    /// </summary>
    Task LogTaskCompletionAsync(string userId, long taskId, string? notes = null);

    /// <summary>
    /// Logs a preference update event.
    /// </summary>
    Task LogPreferenceUpdateAsync(string userId, string preferenceType, string oldValue, string newValue);

    /// <summary>
    /// Logs a notification interaction event.
    /// </summary>
    Task LogNotificationInteractionAsync(string userId, long notificationId, string action);

    /// <summary>
    /// Logs a security event.
    /// </summary>
    Task LogSecurityEventAsync(string userId, string eventType, string details, bool isSuccessful);

    /// <summary>
    /// Logs a performance metric.
    /// </summary>
    Task LogPerformanceMetricAsync(string userId, string action, long elapsedMilliseconds, string? details = null);
}