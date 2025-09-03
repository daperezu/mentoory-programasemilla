using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.Enums;

namespace LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;

public class PackageVersion : AuditableEntity
{
    public PackageVersion()
    {
    }

    public PackageVersion(long id, string label, IEnumerable<PackageVersionLimit> limits, IAuditContext auditContext)
    {
        Id = id;
        Label = label;
        ((List<PackageVersionLimit>)PackageVersionLimits).AddRange(limits);
        SetCreated(auditContext);
    }

    public PackageVersion(string label, IEnumerable<PackageVersionLimit> limits, IAuditContext auditContext)
    {
        Label = label;
        ((List<PackageVersionLimit>)PackageVersionLimits).AddRange(limits);
        SetCreated(auditContext);
    }

    public virtual ICollection<BusinessIncubatorPackage> BusinessIncubatorPackages { get; set; } = [];

    public string Label { get; set; }

    public virtual Package Package { get; set; }

    public long PackageId { get; set; }

    public virtual ICollection<PackageVersionLimit> PackageVersionLimits { get; set; } = [];

    public int GetLimit(PackageLimitType type)
    {
        return PackageVersionLimits.FirstOrDefault(l => l.Type == type)?.Quantity ?? 0;
    }

    public void UpdateLabel(string label, IAuditContext auditContext)
    {
        Label = label;
        SetUpdated(auditContext);
    }

    public void UpdateLimits(IEnumerable<PackageVersionLimit> limits)
    {
        PackageVersionLimits.Clear();
        ((List<PackageVersionLimit>)PackageVersionLimits).AddRange(limits);
    }
}
