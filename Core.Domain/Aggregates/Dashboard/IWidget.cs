namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// Interface for dashboard widgets.
/// </summary>
public interface IWidget
{
    /// <summary>
    /// Gets the widget unique identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the widget type.
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Gets the widget title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the widget description.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the widget data.
    /// </summary>
    object? Data { get; }

    /// <summary>
    /// Gets the widget size.
    /// </summary>
    WidgetSize Size { get; }

    /// <summary>
    /// Gets the widget position.
    /// </summary>
    int Position { get; }

    /// <summary>
    /// Gets a value indicating whether the widget is visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets a value indicating whether the widget is resizable.
    /// </summary>
    bool IsResizable { get; }

    /// <summary>
    /// Gets a value indicating whether the widget is draggable.
    /// </summary>
    bool IsDraggable { get; }

    /// <summary>
    /// Gets a value indicating whether the widget is refreshable.
    /// </summary>
    bool IsRefreshable { get; }

    /// <summary>
    /// Gets the widget refresh interval in seconds.
    /// </summary>
    int? RefreshInterval { get; }

    /// <summary>
    /// Load widget data.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the widget data.</returns>
    Task<object?> LoadDataAsync();

    /// <summary>
    /// Refresh widget data.
    /// </summary>
    /// <returns>A task that represents the asynchronous refresh operation.</returns>
    Task RefreshAsync();

    /// <summary>
    /// Validate widget configuration.
    /// </summary>
    /// <returns>A value indicating whether the widget configuration is valid.</returns>
    bool ValidateConfiguration();

    /// <summary>
    /// Get widget metadata.
    /// </summary>
    /// <returns>A dictionary containing the widget metadata.</returns>
    Dictionary<string, object> GetMetadata();
}