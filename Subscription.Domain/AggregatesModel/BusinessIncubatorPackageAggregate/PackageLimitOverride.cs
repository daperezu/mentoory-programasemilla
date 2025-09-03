using LinaSys.Subscription.Domain.Enums;

namespace LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;

public class PackageLimitOverride
{
    public PackageLimitOverride()
    {
    }

    public PackageLimitOverride(PackageLimitType type, int quantity)
    {
        Type = type;
        Quantity = quantity;
    }

    public virtual BusinessIncubatorPackage BusinessIncubatorPackage { get; set; }

    public long BusinessIncubatorPackageId { get; set; }

    public int Quantity { get; private set; }

    public PackageLimitType Type { get; private set; }
}
