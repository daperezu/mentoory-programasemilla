using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// Dashboard user preferences value object.
/// </summary>
public class DashboardPreferences() : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardPreferences"/> class.
    /// </summary>
    /// <param name="theme">The dashboard theme.</param>
    /// <param name="language">The dashboard language.</param>
    /// <param name="refreshInterval">The refresh interval in seconds.</param>
    /// <param name="showNotifications">A value indicating whether to show notifications.</param>
    /// <param name="playNotificationSound">A value indicating whether to play sound for notifications.</param>
    /// <param name="showTaskReminders">A value indicating whether to show task reminders.</param>
    /// <param name="autoRefreshEnabled">A value indicating whether auto-refresh is enabled.</param>
    /// <param name="compactView">A value indicating whether compact view mode is enabled.</param>
    /// <param name="showWidgetHeaders">A value indicating whether to show widget headers.</param>
    /// <param name="enableAnimations">A value indicating whether animations are enabled.</param>
    /// <param name="dateFormat">The date format.</param>
    /// <param name="timeFormat">The time format.</param>
    /// <param name="timezone">The timezone.</param>
    /// <param name="widgetLayout">The widget layout (JSON).</param>
    public DashboardPreferences(
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
        string timezone,
        string? widgetLayout = null)
        : this()
    {
        Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        Language = language ?? throw new ArgumentNullException(nameof(language));
        RefreshInterval = refreshInterval > 0 ? refreshInterval : throw new ArgumentException("Refresh interval must be greater than 0", nameof(refreshInterval));
        ShowNotifications = showNotifications;
        PlayNotificationSound = playNotificationSound;
        ShowTaskReminders = showTaskReminders;
        AutoRefreshEnabled = autoRefreshEnabled;
        CompactView = compactView;
        ShowWidgetHeaders = showWidgetHeaders;
        EnableAnimations = enableAnimations;
        DateFormat = dateFormat ?? throw new ArgumentNullException(nameof(dateFormat));
        TimeFormat = timeFormat ?? throw new ArgumentNullException(nameof(timeFormat));
        Timezone = timezone ?? throw new ArgumentNullException(nameof(timezone));
        WidgetLayout = widgetLayout;
    }

    /// <summary>
    /// Gets the dashboard theme.
    /// </summary>
    public string Theme { get; private set; } = "light";

    /// <summary>
    /// Gets the dashboard language.
    /// </summary>
    public string Language { get; private set; } = "es";

    /// <summary>
    /// Gets the refresh interval in seconds.
    /// </summary>
    public int RefreshInterval { get; private set; } = 300;

    /// <summary>
    /// Gets a value indicating whether to show notifications.
    /// </summary>
    public bool ShowNotifications { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether to play sound for notifications.
    /// </summary>
    public bool PlayNotificationSound { get; private set; } = false;

    /// <summary>
    /// Gets a value indicating whether to show task reminders.
    /// </summary>
    public bool ShowTaskReminders { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether auto-refresh is enabled.
    /// </summary>
    public bool AutoRefreshEnabled { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether compact view mode is enabled.
    /// </summary>
    public bool CompactView { get; private set; } = false;

    /// <summary>
    /// Gets a value indicating whether to show widget headers.
    /// </summary>
    public bool ShowWidgetHeaders { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether animations are enabled.
    /// </summary>
    public bool EnableAnimations { get; private set; } = true;

    /// <summary>
    /// Gets the date format.
    /// </summary>
    public string DateFormat { get; private set; } = "DD/MM/YYYY";

    /// <summary>
    /// Gets the time format.
    /// </summary>
    public string TimeFormat { get; private set; } = "HH:mm";

    /// <summary>
    /// Gets the timezone.
    /// </summary>
    public string Timezone { get; private set; } = "America/Mexico_City";

    /// <summary>
    /// Gets the widget layout (JSON).
    /// </summary>
    public string? WidgetLayout { get; private set; }

    /// <summary>
    /// Create preferences with updated theme.
    /// </summary>
    /// <param name="theme">The new theme.</param>
    /// <returns>A new instance of <see cref="DashboardPreferences"/> with the updated theme.</returns>
    public DashboardPreferences WithTheme(string theme)
    {
        return new DashboardPreferences(
            theme,
            Language,
            RefreshInterval,
            ShowNotifications,
            PlayNotificationSound,
            ShowTaskReminders,
            AutoRefreshEnabled,
            CompactView,
            ShowWidgetHeaders,
            EnableAnimations,
            DateFormat,
            TimeFormat,
            Timezone,
            WidgetLayout);
    }

    /// <summary>
    /// Create preferences with updated language.
    /// </summary>
    /// <param name="language">The new language.</param>
    /// <returns>A new instance of <see cref="DashboardPreferences"/> with the updated language.</returns>
    public DashboardPreferences WithLanguage(string language)
    {
        return new DashboardPreferences(
            Theme,
            language,
            RefreshInterval,
            ShowNotifications,
            PlayNotificationSound,
            ShowTaskReminders,
            AutoRefreshEnabled,
            CompactView,
            ShowWidgetHeaders,
            EnableAnimations,
            DateFormat,
            TimeFormat,
            Timezone,
            WidgetLayout);
    }

    /// <summary>
    /// Create preferences with updated refresh interval.
    /// </summary>
    /// <param name="seconds">The refresh interval in seconds.</param>
    /// <returns>A new instance of <see cref="DashboardPreferences"/> with the updated refresh interval.</returns>
    public DashboardPreferences WithRefreshInterval(int seconds)
    {
        return new DashboardPreferences(
            Theme,
            Language,
            seconds,
            ShowNotifications,
            PlayNotificationSound,
            ShowTaskReminders,
            AutoRefreshEnabled,
            CompactView,
            ShowWidgetHeaders,
            EnableAnimations,
            DateFormat,
            TimeFormat,
            Timezone,
            WidgetLayout);
    }

    /// <summary>
    /// Create preferences with updated widget layout.
    /// </summary>
    /// <param name="widgetLayout">The new widget layout (JSON).</param>
    /// <returns>A new instance of <see cref="DashboardPreferences"/> with the updated widget layout.</returns>
    public DashboardPreferences WithWidgetLayout(string? widgetLayout)
    {
        return new DashboardPreferences(
            Theme,
            Language,
            RefreshInterval,
            ShowNotifications,
            PlayNotificationSound,
            ShowTaskReminders,
            AutoRefreshEnabled,
            CompactView,
            ShowWidgetHeaders,
            EnableAnimations,
            DateFormat,
            TimeFormat,
            Timezone,
            widgetLayout);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Theme;
        yield return Language;
        yield return RefreshInterval;
        yield return ShowNotifications;
        yield return PlayNotificationSound;
        yield return ShowTaskReminders;
        yield return AutoRefreshEnabled;
        yield return CompactView;
        yield return ShowWidgetHeaders;
        yield return EnableAnimations;
        yield return DateFormat;
        yield return TimeFormat;
        yield return Timezone;
        yield return WidgetLayout ?? string.Empty;
    }
}
