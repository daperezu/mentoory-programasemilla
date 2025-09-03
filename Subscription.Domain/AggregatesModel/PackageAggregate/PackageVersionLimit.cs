using LinaSys.Subscription.Domain.Enums;

namespace LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;

public class PackageVersionLimit
{
    public PackageVersionLimit()
    {
    }

    public PackageVersionLimit(PackageLimitType type, int quantity)
    {
        Type = type;
        Quantity = quantity;
    }

    public long PackageVersionId { get; private set; }

    public PackageLimitType Type { get; private set; }

    public int Quantity { get; private set; }

    public virtual PackageVersion PackageVersion { get; set; }
}
