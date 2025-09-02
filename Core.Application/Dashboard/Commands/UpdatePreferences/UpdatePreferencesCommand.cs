using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Dashboard.Commands.UpdatePreferences;

public class UpdatePreferencesCommand(
    string userId,
    string theme,
    string language,
    int refreshInterval,
    bool showNotifications,
    bool playNotificationSound,
    bool showTaskReminders,
    bool autoRefreshEnabled,
    bool compactView,
    bool showWidgetHeaders,
    bool enableAnimations,
    string dateFormat,
    string timeFormat,
    string timezone) : IBaseRequest
{
    public string UserId { get; } = userId;
    public string Theme { get; } = theme;
    public string Language { get; } = language;
    public int RefreshInterval { get; } = refreshInterval;
    public bool ShowNotifications { get; } = showNotifications;
    public bool PlayNotificationSound { get; } = playNotificationSound;
    public bool ShowTaskReminders { get; } = showTaskReminders;
    public bool AutoRefreshEnabled { get; } = autoRefreshEnabled;
    public bool CompactView { get; } = compactView;
    public bool ShowWidgetHeaders { get; } = showWidgetHeaders;
    public bool EnableAnimations { get; } = enableAnimations;
    public string DateFormat { get; } = dateFormat;
    public string TimeFormat { get; } = timeFormat;
    public string Timezone { get; } = timezone;
}