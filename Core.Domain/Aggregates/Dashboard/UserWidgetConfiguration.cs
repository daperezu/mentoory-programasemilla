using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// Represents a user's widget configuration settings.
/// </summary>
public class UserWidgetConfiguration : Entity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserWidgetConfiguration"/> class.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="widgetId">The widget identifier.</param>
    /// <param name="position">The widget position.</param>
    /// <param name="width">The widget width.</param>
    /// <param name="height">The widget height.</param>
    /// <param name="isVisible">A value indicating whether the widget is visible.</param>
    /// <param name="configuration">The widget configuration.</param>
    public UserWidgetConfiguration(
        string userId,
        long widgetId,
        int position,
        int width = 4,
        int height = 2,
        bool isVisible = true,
        string? configuration = null)
        : this()
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        WidgetId = widgetId;
        Position = position >= 0 ? position : throw new ArgumentException("Position must be non-negative", nameof(position));
        Width = width > 0 ? width : throw new ArgumentException("Width must be positive", nameof(width));
        Height = height > 0 ? height : throw new ArgumentException("Height must be positive", nameof(height));
        IsVisible = isVisible;
        Configuration = configuration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserWidgetConfiguration"/> class.
    /// </summary>
    protected UserWidgetConfiguration()
    {
        UserId = string.Empty;
        Position = 0;
        Width = 4;
        Height = 2;
        IsVisible = true;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Gets the widget identifier.
    /// </summary>
    public long WidgetId { get; private set; }

    /// <summary>
    /// Gets the widget position.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Gets the widget width.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the widget height.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the widget is visible.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Gets the widget configuration.
    /// </summary>
    public string? Configuration { get; private set; }

    /// <summary>
    /// Gets the created date.
    /// </summary>
    public DateTime CreatedDate { get; private set; }

    /// <summary>
    /// Gets the modified date.
    /// </summary>
    public DateTime? ModifiedDate { get; private set; }

    /// <summary>
    /// Updates the widget position.
    /// </summary>
    /// <param name="position">The new position.</param>
    public void UpdatePosition(int position)
    {
        Position = position >= 0 ? position : throw new ArgumentException("Position must be non-negative", nameof(position));
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the widget size.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    public void UpdateSize(int width, int height)
    {
        Width = width > 0 ? width : throw new ArgumentException("Width must be positive", nameof(width));
        Height = height > 0 ? height : throw new ArgumentException("Height must be positive", nameof(height));
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the widget visibility.
    /// </summary>
    /// <param name="isVisible">A value indicating whether the widget is visible.</param>
    public void UpdateVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the widget configuration.
    /// </summary>
    /// <param name="configuration">The new configuration.</param>
    public void UpdateConfiguration(string? configuration)
    {
        Configuration = configuration;
        ModifiedDate = DateTime.UtcNow;
    }
}