namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// User-specific dashboard implementation.
/// </summary>
public class UserDashboard : BaseDashboard
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserDashboard"/> class.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role identifier.</param>
    /// <param name="createdDate">The created date.</param>
    public UserDashboard(string userId, string role, DateTime createdDate)
        : base(userId, role, createdDate)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDashboard"/> class.
    /// </summary>
    protected UserDashboard()
        : base()
    {
    }

    /// <inheritdoc/>
    public override DashboardMetrics GetMetrics()
    {
        return new DefaultDashboardMetrics(
            0, // Overall progress
            0, // Pending items
            0, // Completed items
            0, // Total items
            GetUnreadNotificationsCount(),
            LastActivityDate,
            0, // Days since start
            "active"); // Current phase
    }

    /// <summary>
    /// Applies a role dashboard template to this dashboard.
    /// </summary>
    /// <param name="template">The template to apply.</param>
    public void ApplyTemplate(RoleDashboardTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);

        Layout = template.DefaultLayout;
        Theme = template.DefaultTheme;
        Language = template.DefaultLanguage;

        // Apply default preferences from template
        var preferences = new DashboardPreferences(
            template.DefaultTheme,
            template.DefaultLanguage,
            template.DefaultRefreshInterval,
            true, // ShowNotifications
            false, // PlayNotificationSound
            true, // ShowTaskReminders
            true, // AutoRefreshEnabled
            false, // CompactView
            true, // ShowWidgetHeaders
            true, // EnableAnimations
            "DD/MM/YYYY",
            "HH:mm",
            "America/Mexico_City",
            template.DefaultLayout);

        UpdatePreferences(preferences);
    }

    /// <summary>
    /// Updates the last activity date for this dashboard.
    /// </summary>
    /// <param name="activityDate">The activity date.</param>
    public void UpdateLastActivityDate(DateTime activityDate)
    {
        UpdateLastActivity(activityDate);
    }
}
