using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Notification.Domain.EmailTemplates;

/// <summary>
/// Repository interface for email templates.
/// </summary>
public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
    /// <summary>
    /// Gets an email template by its key.
    /// </summary>
    /// <param name="key">The template key.</param>
    /// <returns>The email template or null if not found.</returns>
    Task<EmailTemplate?> GetByKeyAsync(string key);

    /// <summary>
    /// Gets all active templates.
    /// </summary>
    /// <returns>A list of active email templates.</returns>
    Task<IEnumerable<EmailTemplate>> GetActiveTemplatesAsync();

    /// <summary>
    /// Gets templates by category.
    /// </summary>
    /// <param name="category">The category name.</param>
    /// <returns>A list of email templates in the category.</returns>
    Task<IEnumerable<EmailTemplate>> GetByCategoryAsync(string category);

    /// <summary>
    /// Adds a new email template.
    /// </summary>
    /// <param name="template">The template to add.</param>
    /// <returns></returns>
    EmailTemplate Add(EmailTemplate template);

    /// <summary>
    /// Updates an existing email template.
    /// </summary>
    /// <param name="template">The template to update.</param>
    void Update(EmailTemplate template);

    /// <summary>
    /// Deletes an email template.
    /// </summary>
    /// <param name="template">The template to delete.</param>
    void Delete(EmailTemplate template);

    /// <summary>
    /// Gets all email templates.
    /// </summary>
    /// <returns>A list of all email templates.</returns>
    Task<IEnumerable<EmailTemplate>> GetAllAsync();
}