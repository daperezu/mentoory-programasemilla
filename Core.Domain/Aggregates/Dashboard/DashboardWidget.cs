using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// Represents a widget in a dashboard.
/// </summary>
public class DashboardWidget : Entity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardWidget"/> class.
    /// </summary>
    /// <param name="widgetId">The widget ID reference.</param>
    /// <param name="name">The widget name.</param>
    /// <param name="type">The widget type.</param>
    /// <param name="gridRow">The grid row position.</param>
    /// <param name="gridColumn">The grid column position.</param>
    /// <param name="width">The widget width in grid columns.</param>
    /// <param name="height">The widget height in grid rows.</param>
    /// <param name="size">The widget size.</param>
    /// <param name="configuration">The widget configuration (JSON).</param>
    public DashboardWidget(
        long widgetId,
        string name,
        string type,
        int gridRow,
        int gridColumn,
        int width,
        int height,
        WidgetSize size,
        string? configuration = null)
        : this()
    {
        WidgetId = widgetId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        GridRow = gridRow;
        GridColumn = gridColumn;
        Width = width > 0 ? width : throw new ArgumentException("Width must be greater than 0", nameof(width));
        Height = height > 0 ? height : throw new ArgumentException("Height must be greater than 0", nameof(height));
        Size = size;
        Configuration = configuration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardWidget"/> class.
    /// </summary>
    protected DashboardWidget()
    {
        Name = string.Empty;
        Type = string.Empty;
        Size = WidgetSize.Medium;
        Width = 1;
        Height = 1;
        IsActive = true;
        IsVisible = true;
        IsCollapsed = false;
    }

    /// <summary>
    /// Gets the widget ID reference.
    /// </summary>
    public long WidgetId { get; private set; }

    /// <summary>
    /// Gets the widget name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the widget type.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the grid row position.
    /// </summary>
    public int GridRow { get; private set; }

    /// <summary>
    /// Gets the grid column position.
    /// </summary>
    public int GridColumn { get; private set; }

    /// <summary>
    /// Gets the widget width in grid columns.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the widget height in grid rows.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the widget size.
    /// </summary>
    public WidgetSize Size { get; private set; }

    /// <summary>
    /// Gets the widget configuration (JSON).
    /// </summary>
    public string? Configuration { get; private set; }

    /// <summary>
    /// Gets the widget component name.
    /// </summary>
    public string? Component { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the widget is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the widget is visible.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the widget is collapsed.
    /// </summary>
    public bool IsCollapsed { get; private set; }

    /// <summary>
    /// Gets the refresh interval override in seconds.
    /// </summary>
    public int? RefreshInterval { get; private set; }

    /// <summary>
    /// Gets the last refreshed timestamp.
    /// </summary>
    public DateTime? LastRefreshedAt { get; private set; }

    /// <summary>
    /// Gets the role IDs that can access this widget.
    /// </summary>
    public string? Roles { get; private set; }

    /// <summary>
    /// Gets the default position for ordering.
    /// </summary>
    public int DefaultPosition { get; private set; }

    /// <summary>
    /// Update widget position.
    /// </summary>
    /// <param name="gridRow">The grid row position.</param>
    /// <param name="gridColumn">The grid column position.</param>
    public void UpdatePosition(int gridRow, int gridColumn)
    {
        GridRow = gridRow;
        GridColumn = gridColumn;
    }

    /// <summary>
    /// Update widget size.
    /// </summary>
    /// <param name="width">The widget width in grid columns.</param>
    /// <param name="height">The widget height in grid rows.</param>
    /// <param name="size">The widget size.</param>
    public void UpdateSize(int width, int height, WidgetSize size)
    {
        Width = width > 0 ? width : throw new ArgumentException("Width must be greater than 0", nameof(width));
        Height = height > 0 ? height : throw new ArgumentException("Height must be greater than 0", nameof(height));
        Size = size;
    }

    /// <summary>
    /// Update widget configuration.
    /// </summary>
    /// <param name="configuration">The widget configuration (JSON).</param>
    public void UpdateConfiguration(string? configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Show widget.
    /// </summary>
    public void Show()
    {
        IsVisible = true;
    }

    /// <summary>
    /// Hide widget.
    /// </summary>
    public void Hide()
    {
        IsVisible = false;
    }

    /// <summary>
    /// Collapse widget.
    /// </summary>
    public void Collapse()
    {
        IsCollapsed = true;
    }

    /// <summary>
    /// Expand widget.
    /// </summary>
    public void Expand()
    {
        IsCollapsed = false;
    }

    /// <summary>
    /// Toggle visibility.
    /// </summary>
    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }

    /// <summary>
    /// Toggle collapse state.
    /// </summary>
    public void ToggleCollapse()
    {
        IsCollapsed = !IsCollapsed;
    }

    /// <summary>
    /// Set refresh interval.
    /// </summary>
    /// <param name="seconds">The refresh interval in seconds.</param>
    public void SetRefreshInterval(int? seconds)
    {
        if (seconds.HasValue && seconds.Value <= 0)
        {
            throw new ArgumentException("Refresh interval must be greater than 0", nameof(seconds));
        }

        RefreshInterval = seconds;
    }

    /// <summary>
    /// Mark as refreshed.
    /// </summary>
    public void MarkAsRefreshed()
    {
        LastRefreshedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if refresh is needed.
    /// </summary>
    /// <returns>A value indicating whether refresh is needed.</returns>
    public bool NeedsRefresh()
    {
        if (!RefreshInterval.HasValue || !LastRefreshedAt.HasValue)
        {
            return true;
        }

        var secondsSinceRefresh = (DateTime.UtcNow - LastRefreshedAt.Value).TotalSeconds;
        return secondsSinceRefresh >= RefreshInterval.Value;
    }
}
