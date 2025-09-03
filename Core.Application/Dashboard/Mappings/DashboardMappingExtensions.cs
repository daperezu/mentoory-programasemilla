using LinaSys.Core.Application.Dashboard.Queries.GetDashboard;
using LinaSys.Core.Domain.Aggregates.Dashboard;

namespace LinaSys.Core.Application.Dashboard.Mappings;

public static class DashboardMappingExtensions
{
    public static DashboardDto ToDto(this BaseDashboard dashboard)
    {
        return new DashboardDto
        {
            UserId = dashboard.UserId,
            Role = dashboard.Role,
            Layout = dashboard.Layout ?? string.Empty,
            Theme = dashboard.Theme,
            Language = dashboard.Language,
            LastActivityDate = dashboard.LastActivityDate ?? DateTime.UtcNow,
            Preferences = dashboard.Preferences?.ToDto() ?? new DashboardPreferencesDto(),
            Metrics = new DashboardMetricsDto(), // Will be set separately if needed
            Widgets = [], // Will be set separately
            Notifications = [] // Will be set separately
        };
    }

    public static WidgetDto ToDto(this DashboardWidget widget)
    {
        return new WidgetDto
        {
            Id = widget.Id,
            Code = widget.Name, // Name is used as the code identifier
            Name = widget.Name,
            Description = string.Empty, // Not available in domain entity
            Type = widget.Type,
            Position = widget.DefaultPosition,
            Width = widget.Width,
            Height = widget.Height,
            IsVisible = widget.IsVisible,
            Configuration = widget.Configuration,
            Component = widget.Component ?? string.Empty,
            ApiEndpoint = null, // Not available in domain entity
            RefreshInterval = widget.RefreshInterval,
            Data = null // Will be loaded separately
        };
    }

    public static NotificationDto ToDto(this UserNotification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString(),
            Priority = notification.Priority.ToString(),
            Category = notification.Category.ToString(),
            IsRead = notification.IsRead,
            CreatedDate = notification.CreatedAt,
            ActionUrl = notification.ActionUrl,
            ActionText = notification.ActionText
        };
    }

    public static DashboardPreferencesDto ToDto(this DashboardPreferences preferences)
    {
        return new DashboardPreferencesDto
        {
            Theme = preferences.Theme,
            Language = preferences.Language,
            RefreshInterval = preferences.RefreshInterval,
            ShowNotifications = preferences.ShowNotifications,
            PlayNotificationSound = preferences.PlayNotificationSound,
            ShowTaskReminders = preferences.ShowTaskReminders,
            AutoRefreshEnabled = preferences.AutoRefreshEnabled,
            CompactView = preferences.CompactView,
            ShowWidgetHeaders = preferences.ShowWidgetHeaders,
            EnableAnimations = preferences.EnableAnimations,
            DateFormat = preferences.DateFormat,
            TimeFormat = preferences.TimeFormat,
            Timezone = preferences.Timezone
        };
    }

    public static DashboardMetricsDto ToDto(this DashboardMetrics metrics)
    {
        return new DashboardMetricsDto
        {
            TotalItems = metrics.TotalItems,
            PendingItems = metrics.PendingItems,
            CompletedItems = metrics.CompletedItems,
            UnreadNotifications = metrics.UnreadNotifications,
            LastActivityDate = metrics.LastActivityDate,
            OverallProgress = 0, // Calculate as needed
            DaysSinceStart = 0, // Calculate as needed
            CurrentPhase = string.Empty, // Set as needed
            CustomMetrics = []
        };
    }

    public static List<WidgetDto> ToDto(this IEnumerable<DashboardWidget> widgets)
    {
        return widgets.Select(w => w.ToDto()).ToList();
    }

    public static List<NotificationDto> ToDto(this IEnumerable<UserNotification> notifications)
    {
        return notifications.Select(n => n.ToDto()).ToList();
    }
}
