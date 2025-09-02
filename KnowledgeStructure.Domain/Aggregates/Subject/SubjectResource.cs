using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.Subject;

public class SubjectResource : Entity
{
    public SubjectResource(string title, string url, string type, int order, int? estimatedMinutes = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be empty.", nameof(url));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Type cannot be empty.", nameof(type));
        }

        if (order <= 0)
        {
            throw new ArgumentException("Order must be greater than zero.", nameof(order));
        }

        Title = title.Trim();
        Url = url.Trim();
        Type = type.Trim();
        Order = order;
        EstimatedMinutes = estimatedMinutes;
    }

    private SubjectResource()
    {
        // Required by EF Core
    }

    public string Title { get; private set; }

    public string Url { get; private set; }

    public string Type { get; private set; }

    public int? EstimatedMinutes { get; private set; }

    public int Order { get; private set; }

    public void Update(string title, string url, string type, int? estimatedMinutes)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be empty.", nameof(url));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Type cannot be empty.", nameof(type));
        }

        Title = title.Trim();
        Url = url.Trim();
        Type = type.Trim();
        EstimatedMinutes = estimatedMinutes;
    }

    // Remove navigation property - this is owned by Subject aggregate
    // public virtual Subject Subject { get; set; }
    internal void UpdateOrder(int newOrder)
    {
        if (newOrder <= 0)
        {
            throw new ArgumentException("Order must be greater than zero.", nameof(newOrder));
        }

        Order = newOrder;
    }
}
