using LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.Subject;

public class Subject : Entity, IAggregateRoot
{
    // Remove reference to KnowledgeStructureSubjects - Subject is its own aggregate
    // Relationships to Topics are managed by KnowledgeStructure aggregate
    private readonly List<SubjectResource> _subjectResources = [];

    public Subject(string title, string? content = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        Title = title.Trim();
        Content = content?.Trim();
    }

    public string Title { get; private set; }

    public string? Content { get; private set; }

    public IReadOnlyCollection<SubjectResource> SubjectResources => _subjectResources.AsReadOnly();

    public void AddResource(string title, string url, string type, int? estimatedMinutes = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Resource title cannot be empty.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Resource URL cannot be empty.", nameof(url));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Resource type cannot be empty.", nameof(type));
        }

        var order = _subjectResources.Count > 0 ? _subjectResources.Max(r => r.Order) + 1 : 1;
        var resource = new SubjectResource(title, url, type, order, estimatedMinutes);
        _subjectResources.Add(resource);
    }

    public void ClearResources()
    {
        _subjectResources.Clear();
    }

    public void Update(string title, string? content)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        Title = title.Trim();
        Content = content?.Trim();
    }

    public void RemoveResource(long resourceId)
    {
        var resource = _subjectResources.FirstOrDefault(r => r.Id == resourceId);
        if (resource is not null)
        {
            _subjectResources.Remove(resource);
            ReorderResources();
        }
    }

    private void ReorderResources()
    {
        var orderedResources = _subjectResources.OrderBy(r => r.Order).ToList();
        for (int i = 0; i < orderedResources.Count; i++)
        {
            orderedResources[i].UpdateOrder(i + 1);
        }
    }
}
