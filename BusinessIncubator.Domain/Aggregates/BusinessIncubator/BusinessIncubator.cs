using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class BusinessIncubator : SoftDeletableEntity, IAggregateRoot
{
    public BusinessIncubator(string name, string? description, string key, IAuditContext auditableContext)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be empty.", nameof(key));
        }

        Name = name.Trim();
        Description = description?.Trim();
        Key = key.Trim();

        ExternalId = Guid.NewGuid();

        SetCreated(auditableContext);
    }

    protected BusinessIncubator()
    {
    }

    public Guid ExternalId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public string Key { get; private set; }

    public BusinessIncubatorStatus Status { get; private set; }

    // Navigation property for EF Core - not part of the aggregate
    internal virtual ICollection<Project> Projects { get; private set; } = [];

    public void ChangeStatus(BusinessIncubatorStatus requestStatus, IAuditContext auditableContext)
    {
        EnsureNotDeleted();

        Status = requestStatus;
        SetUpdated(auditableContext);
    }

    public void Update(string name, string? description, string key, IAuditContext auditableContext)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be empty.", nameof(key));
        }

        Name = name.Trim();
        Description = description?.Trim();
        Key = key.Trim();

        SetUpdated(auditableContext);
    }
}
