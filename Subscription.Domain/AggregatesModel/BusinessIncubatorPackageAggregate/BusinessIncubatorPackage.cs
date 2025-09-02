using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using LinaSys.Subscription.Domain.Enums;

namespace LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;

public class BusinessIncubatorPackage : AuditableEntity, IAggregateRoot
{
    public BusinessIncubatorPackage()
    {
    }

    public BusinessIncubatorPackage(long businessIncubatorId, long packageVersionId, IAuditContext auditContext)
    {
        BusinessIncubatorId = businessIncubatorId;
        PackageVersionId = packageVersionId;
        SetCreated(auditContext);
    }

    public long BusinessIncubatorId { get; private set; }

    public virtual ICollection<PackageLimitOverride> PackageLimitOverrides { get; private set; } = [];

    public virtual PackageVersion PackageVersion { get; set; }

    public long PackageVersionId { get; private set; }

    public void AddExtraLimit(PackageLimitType type, int quantity, IAuditContext auditContext)
    {
        PackageLimitOverrides.Add(new PackageLimitOverride(type, quantity));
        SetUpdated(auditContext);
    }

    public void ClearExtraLimits(IAuditContext auditContext)
    {
        PackageLimitOverrides.Clear();
        SetUpdated(auditContext);
    }

    public int GetEffectiveLimit(PackageLimitType type)
    {
        var baseLimit = PackageVersion.GetLimit(type);
        var extra = PackageLimitOverrides
            .Where(o => o.Type == type)
            .Sum(o => o.Quantity);
        return baseLimit + extra;
    }

    public void DeleteExtraLimit(PackageLimitType type, int quantity, IAuditContext auditContext)
    {
        var limit = PackageLimitOverrides.FirstOrDefault(o => o.Type == type && o.Quantity == quantity);
        if (limit is not null)
        {
            PackageLimitOverrides.Remove(limit);
        }

        SetUpdated(auditContext);
    }

    public void SwitchPackageVersion(long packageVersionId, IAuditContext auditContext)
    {
        PackageVersionId = packageVersionId;
        SetUpdated(auditContext);
    }
}
