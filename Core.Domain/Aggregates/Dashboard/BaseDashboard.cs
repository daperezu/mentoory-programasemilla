using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// Base class for all role-specific dashboards.
/// </summary>
public abstract class BaseDashboard() : Entity, IAggregateRoot
{
    private readonly List<DashboardWidget> _widgets = [];
    private readonly List<UserNotification> _notifications = [];

    protected BaseDashboard(string userId, string role, DateTime createdDate)
        : this()
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Role = role ?? throw new ArgumentNullException(nameof(role));
        CreatedDate = createdDate;
    }

    /// <summary>
    /// Gets or sets the user ID associated with this dashboard.
    /// </summary>
    public string UserId { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role ID for this dashboard configuration.
    /// </summary>
    public string Role { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dashboard layout configuration (JSON).
    /// </summary>
    public string? Layout { get; protected set; }

    /// <summary>
    /// Gets or sets the dashboard theme.
    /// </summary>
    public string Theme { get; protected set; } = "light";

    /// <summary>
    /// Gets or sets the dashboard language.
    /// </summary>
    public string Language { get; protected set; } = "es";

    /// <summary>
    /// Gets or sets the refresh interval in seconds.
    /// </summary>
    public int RefreshInterval { get; protected set; } = 300;

    /// <summary>
    /// Gets or sets a value indicating whether this dashboard is active.
    /// </summary>
    public bool IsActive { get; protected set; } = true;

    /// <summary>
    /// Gets the dashboard widgets.
    /// </summary>
    public IReadOnlyList<DashboardWidget> Widgets => _widgets.AsReadOnly();

    /// <summary>
    /// Gets the user notifications.
    /// </summary>
    public IReadOnlyList<UserNotification> Notifications => _notifications.AsReadOnly();

    /// <summary>
    /// Gets or sets the dashboard preferences.
    /// </summary>
    public DashboardPreferences Preferences { get; protected set; } = new();

    /// <summary>
    /// Gets or sets the last activity date for this dashboard.
    /// </summary>
    public DateTime? LastActivityDate { get; protected set; }

    /// <summary>
    /// Gets or sets the created date for this dashboard.
    /// </summary>
    public DateTime CreatedDate { get; protected set; }

    /// <summary>
    /// Gets the dashboard metrics.
    /// </summary>
    /// <returns>The dashboard metrics.</returns>
    public abstract DashboardMetrics GetMetrics();

    /// <summary>
    /// Update dashboard preferences.
    /// </summary>
    /// <param name="preferences">The dashboard preferences to update.</param>
    public virtual void UpdatePreferences(DashboardPreferences preferences)
    {
        Preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        Theme = preferences.Theme;
        Language = preferences.Language;
        RefreshInterval = preferences.RefreshInterval;
    }

    /// <summary>
    /// Update dashboard layout.
    /// </summary>
    /// <param name="layout">The layout configuration string.</param>
    public virtual void UpdateLayout(string layout)
    {
        Layout = layout;
    }

    /// <summary>
    /// Add a widget to the dashboard.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    public virtual void AddWidget(DashboardWidget widget)
    {
        ArgumentNullException.ThrowIfNull(widget);

        if (_widgets.Any(w => w.WidgetId == widget.WidgetId))
        {
            throw new InvalidOperationException($"Widget {widget.WidgetId} already exists in dashboard");
        }

        _widgets.Add(widget);
    }

    /// <summary>
    /// Remove a widget from the dashboard.
    /// </summary>
    /// <param name="widgetId">The ID of the widget to remove.</param>
    public virtual void RemoveWidget(long widgetId)
    {
        var widget = _widgets.FirstOrDefault(w => w.WidgetId == widgetId);
        if (widget is not null)
        {
            _widgets.Remove(widget);
        }
    }

    /// <summary>
    /// Update widget configuration.
    /// </summary>
    /// <param name="widgetId">The ID of the widget to update.</param>
    /// <param name="configuration">The new configuration for the widget.</param>
    public virtual void UpdateWidgetConfiguration(long widgetId, string configuration)
    {
        var widget = _widgets.FirstOrDefault(w => w.WidgetId == widgetId);
        widget?.UpdateConfiguration(configuration);
    }

    /// <summary>
    /// Add a notification.
    /// </summary>
    /// <param name="notification">The notification to add.</param>
    public virtual void AddNotification(UserNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        _notifications.Add(notification);
    }

    /// <summary>
    /// Mark notification as read.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to mark as read.</param>
    public virtual void MarkNotificationAsRead(long notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        notification?.MarkAsRead();
    }

    /// <summary>
    /// Dismiss notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to dismiss.</param>
    public virtual void DismissNotification(long notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        notification?.Dismiss();
    }

    /// <summary>
    /// Get unread notifications count.
    /// </summary>
    /// <returns>The count of unread notifications.</returns>
    public virtual int GetUnreadNotificationsCount()
    {
        return _notifications.Count(n => !n.IsRead && !n.IsDismissed);
    }

    /// <summary>
    /// Activate dashboard.
    /// </summary>
    public virtual void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivate dashboard.
    /// </summary>
    public virtual void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Update last activity date.
    /// </summary>
    /// <param name="activityDate">The activity date to set.</param>
    protected virtual void UpdateLastActivity(DateTime activityDate)
    {
        LastActivityDate = activityDate;
    }
}
