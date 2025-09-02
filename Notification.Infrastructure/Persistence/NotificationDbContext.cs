using LinaSys.Notification.Domain.EmailTemplates;
using LinaSys.Notification.Infrastructure.Persistence.EntityConfigurations;
using LinaSys.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Notification.Infrastructure.Persistence;

/// <summary>
/// Database context for the Notification module.
/// </summary>
public class NotificationDbContext(DbContextOptions<NotificationDbContext> options, IMediator mediator) : SharedAbstractDbContext(options, mediator)
{
    public DbSet<EmailTemplate> EmailTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new EmailTemplateConfiguration());

        // Set default schema for all entities in this context
        modelBuilder.HasDefaultSchema("notification");
    }
}