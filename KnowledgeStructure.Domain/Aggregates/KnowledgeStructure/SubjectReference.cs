using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;

/// <summary>
/// Value object representing a reference to a Subject within a Topic.
/// This maintains the relationship without crossing aggregate boundaries.
/// </summary>
public class SubjectReference : ValueObject
{
    public SubjectReference(long subjectId, int order)
    {
        if (subjectId <= 0)
        {
            throw new ArgumentException("Subject ID must be greater than zero.", nameof(subjectId));
        }

        if (order <= 0)
        {
            throw new ArgumentException("Order must be greater than zero.", nameof(order));
        }

        SubjectId = subjectId;
        Order = order;
    }

    private SubjectReference()
    {
        // Required by EF Core
    }

    public long SubjectId { get; private set; }

    public int Order { get; private set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SubjectId;
        yield return Order;
    }
}
