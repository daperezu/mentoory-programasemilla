using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Dashboard;

public class RoleDashboardTemplate : Entity
{
    public RoleDashboardTemplate(
        string role,
        string defaultLayout,
        string defaultTheme = "light",
        string defaultLanguage = "es",
        int defaultRefreshInterval = 300,
        string? widgetCodes = null)
        : this()
    {
        Role = role ?? throw new ArgumentNullException(nameof(role));
        DefaultLayout = defaultLayout ?? throw new ArgumentNullException(nameof(defaultLayout));
        DefaultTheme = defaultTheme;
        DefaultLanguage = defaultLanguage;
        DefaultRefreshInterval = defaultRefreshInterval > 0 ? defaultRefreshInterval : 300;
        WidgetCodes = widgetCodes;
    }

    protected RoleDashboardTemplate()
    {
        Role = string.Empty;
        DefaultLayout = string.Empty;
        DefaultTheme = "light";
        DefaultLanguage = "es";
        DefaultRefreshInterval = 300;
        IsActive = true;
        CreatedDate = DateTime.UtcNow;
    }

    public string Role { get; private set; }

    public string DefaultLayout { get; private set; }

    public string DefaultTheme { get; private set; }

    public string DefaultLanguage { get; private set; }

    public int DefaultRefreshInterval { get; private set; }

    public string? WidgetCodes { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public DateTime? ModifiedDate { get; private set; }

    public void UpdateLayout(string layout)
    {
        DefaultLayout = layout ?? throw new ArgumentNullException(nameof(layout));
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdateTheme(string theme)
    {
        DefaultTheme = theme ?? throw new ArgumentNullException(nameof(theme));
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdateWidgets(string? widgetCodes)
    {
        WidgetCodes = widgetCodes;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    public List<string> GetWidgetCodes()
    {
        if (string.IsNullOrWhiteSpace(WidgetCodes))
        {
            return [];
        }

        return WidgetCodes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(w => w.Trim())
                         .ToList();
    }
}
