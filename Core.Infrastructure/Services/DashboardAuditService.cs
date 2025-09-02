using LinaSys.Core.Application.Dashboard.Services;
using Microsoft.Extensions.Logging;

namespace LinaSys.Core.Infrastructure.Services;

/// <summary>
/// Implementation of dashboard audit service.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DashboardAuditService"/> class.
/// </remarks>
public class DashboardAuditService(ILogger<DashboardAuditService> logger) : IDashboardAuditService
{

    /// <inheritdoc />
    public Task LogDashboardAccessAsync(string userId, long projectId, string dashboardType)
    {
        logger.LogInformation(
            "Dashboard accessed: User={UserId}, Project={ProjectId}, Type={DashboardType}, Time={Timestamp}",
            userId, projectId, dashboardType, DateTime.UtcNow);

        // TODO: Persist to database when audit table is created
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LogWidgetInteractionAsync(string userId, long widgetId, string action, string? details = null)
    {
        logger.LogInformation(
            "Widget interaction: User={UserId}, Widget={WidgetId}, Action={Action}, Details={Details}, Time={Timestamp}",
            userId, widgetId, action, details, DateTime.UtcNow);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LogTaskCompletionAsync(string userId, long taskId, string? notes = null)
    {
        logger.LogInformation(
            "Task completed: User={UserId}, Task={TaskId}, Notes={Notes}, Time={Timestamp}",
            userId, taskId, notes, DateTime.UtcNow);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LogPreferenceUpdateAsync(string userId, string preferenceType, string oldValue, string newValue)
    {
        logger.LogInformation(
            "Preference updated: User={UserId}, Type={PreferenceType}, OldValue={OldValue}, NewValue={NewValue}, Time={Timestamp}",
            userId, preferenceType, oldValue, newValue, DateTime.UtcNow);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LogNotificationInteractionAsync(string userId, long notificationId, string action)
    {
        logger.LogInformation(
            "Notification interaction: User={UserId}, Notification={NotificationId}, Action={Action}, Time={Timestamp}",
            userId, notificationId, action, DateTime.UtcNow);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LogSecurityEventAsync(string userId, string eventType, string details, bool isSuccessful)
    {
        if (isSuccessful)
        {
            logger.LogInformation(
                "Security event: User={UserId}, Type={EventType}, Details={Details}, Success=true, Time={Timestamp}",
                userId, eventType, details, DateTime.UtcNow);
        }
        else
        {
            logger.LogWarning(
                "Security event failed: User={UserId}, Type={EventType}, Details={Details}, Success=false, Time={Timestamp}",
                userId, eventType, details, DateTime.UtcNow);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LogPerformanceMetricAsync(string userId, string action, long elapsedMilliseconds, string? details = null)
    {
        if (elapsedMilliseconds > 5000)
        {
            logger.LogWarning(
                "Slow performance detected: User={UserId}, Action={Action}, ElapsedMs={ElapsedMs}, Details={Details}, Time={Timestamp}",
                userId, action, elapsedMilliseconds, details, DateTime.UtcNow);
        }
        else
        {
            logger.LogDebug(
                "Performance metric: User={UserId}, Action={Action}, ElapsedMs={ElapsedMs}, Details={Details}, Time={Timestamp}",
                userId, action, elapsedMilliseconds, details, DateTime.UtcNow);
        }

        return Task.CompletedTask;
    }
}
