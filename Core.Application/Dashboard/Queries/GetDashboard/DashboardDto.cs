namespace LinaSys.Core.Application.Dashboard.Queries.GetDashboard;

public class DashboardDto
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Layout { get; set; } = string.Empty;
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "es";
    public List<WidgetDto> Widgets { get; set; } = [];
    public DashboardMetricsDto Metrics { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = [];
    public DashboardPreferencesDto Preferences { get; set; } = new();
    public DateTime LastActivityDate { get; set; }
    public bool IsFirstLogin { get; set; }
}

public class WidgetDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsVisible { get; set; }
    public string? Configuration { get; set; }
    public object? Data { get; set; }
    public string Component { get; set; } = string.Empty;
    public string? ApiEndpoint { get; set; }
    public int? RefreshInterval { get; set; }
}

public class DashboardMetricsDto
{
    public decimal OverallProgress { get; set; }
    public int PendingItems { get; set; }
    public int CompletedItems { get; set; }
    public int TotalItems { get; set; }
    public int UnreadNotifications { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int DaysSinceStart { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public Dictionary<string, object> CustomMetrics { get; set; } = [];
}

public class NotificationDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
}

public class DashboardPreferencesDto
{
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "es";
    public int RefreshInterval { get; set; } = 300;
    public bool ShowNotifications { get; set; } = true;
    public bool PlayNotificationSound { get; set; } = false;
    public bool ShowTaskReminders { get; set; } = true;
    public bool AutoRefreshEnabled { get; set; } = true;
    public bool CompactView { get; set; } = false;
    public bool ShowWidgetHeaders { get; set; } = true;
    public bool EnableAnimations { get; set; } = true;
    public string DateFormat { get; set; } = "DD/MM/YYYY";
    public string TimeFormat { get; set; } = "HH:mm";
    public string Timezone { get; set; } = "America/Mexico_City";
}
