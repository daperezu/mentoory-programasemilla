using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Notification.Domain.EmailTemplates;

/// <summary>
/// Represents an email template in the system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EmailTemplate"/> class.
/// </remarks>
/// <param name="key">The unique key for the template.</param>
/// <param name="name">The display name of the template.</param>
/// <param name="subject">The email subject template.</param>
/// <param name="bodyHtml">The HTML body template.</param>
/// <param name="bodyText">The plain text body template.</param>
/// <param name="description">The template description.</param>
/// <param name="category">The template category.</param>
public class EmailTemplate(
    string key,
    string name,
    string subject,
    string bodyHtml,
    string? bodyText = null,
    string? description = null,
    string? category = null) : Entity, IAggregateRoot
{
    /// <summary>
    /// Gets the unique template key.
    /// </summary>
    public string Key { get; private set; } = key;

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Gets or sets the email subject template.
    /// </summary>
    public string Subject { get; set; } = subject;

    /// <summary>
    /// Gets or sets the HTML body template.
    /// </summary>
    public string BodyHtml { get; set; } = bodyHtml;

    /// <summary>
    /// Gets or sets the plain text body template.
    /// </summary>
    public string? BodyText { get; set; } = bodyText;

    /// <summary>
    /// Gets or sets the template description.
    /// </summary>
    public string? Description { get; set; } = description;

    /// <summary>
    /// Gets or sets the template category.
    /// </summary>
    public string? Category { get; set; } = category;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the template is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updates the template content.
    /// </summary>
    /// <param name="subject">The new subject.</param>
    /// <param name="bodyHtml">The new HTML body.</param>
    /// <param name="bodyText">The new plain text body.</param>
    public void UpdateContent(string subject, string bodyHtml, string? bodyText = null)
    {
        Subject = subject;
        BodyHtml = bodyHtml;
        BodyText = bodyText;
        UpdatedAt = DateTime.UtcNow;
    }
}