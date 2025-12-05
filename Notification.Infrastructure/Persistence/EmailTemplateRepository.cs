using LinaSys.Notification.Domain.EmailTemplates;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Notification.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for email templates.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EmailTemplateRepository"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class EmailTemplateRepository(NotificationDbContext context) : AbstractRepository<EmailTemplate>(context), IEmailTemplateRepository
{
    private readonly NotificationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc/>
    public async Task<EmailTemplate?> GetByKeyAsync(string key)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Key == key);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EmailTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.EmailTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EmailTemplate>> GetByCategoryAsync(string category)
    {
        return await _context.EmailTemplates
            .Where(t => t.Category == category)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EmailTemplate>> GetAllAsync()
    {
        return await _context.EmailTemplates
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public void Delete(EmailTemplate template)
    {
        _context.EmailTemplates.Remove(template);
    }
}
