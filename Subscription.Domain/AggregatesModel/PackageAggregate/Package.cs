using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;

public class Package : AuditableEntity, IAggregateRoot
{
    public Package()
    {
    }

    public Package(string name, IAuditContext auditContext)
    {
        Name = name;
        SetCreated(auditContext);
    }

    public string Name { get; private set; }

    public virtual ICollection<PackageVersion> PackageVersions { get; set; } = [];

    public void AddVersion(PackageVersion version)
    {
        PackageVersions.Add(version);
    }

    public void RemoveVersion(PackageVersion version)
    {
        PackageVersions.Remove(version);
    }

    public void UpdateName(string name, IAuditContext auditContext)
    {
        Name = name;
        SetUpdated(auditContext);
    }

    public void UpsertVersion(PackageVersion version)
    {
        if (version.Id > 0)
        {
            var existing = PackageVersions.FirstOrDefault(v => v.Id == version.Id);
            if (existing is not null)
            {
                RemoveVersion(existing);
            }
            else
            {
                throw new InvalidOperationException($"The package version was not found with the id {version.Id}");
            }
        }

        AddVersion(version);
    }
}
