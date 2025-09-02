namespace LinaSys.Web.Models;

/// <summary>
/// Represents a single item in a breadcrumb navigation.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Gets or sets the display text for the breadcrumb.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL for the breadcrumb link.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this is the active/current breadcrumb.
    /// </summary>
    public bool IsActive { get; set; }
}
