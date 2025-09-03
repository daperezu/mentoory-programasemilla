using LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.Topic;

public class Topic : Entity, IAggregateRoot
{
    private readonly List<KnowledgeStructureTopic> _knowledgeStructureTopics = [];

    public Topic(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Topic name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    private Topic()
    {
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyCollection<KnowledgeStructureTopic> KnowledgeStructureTopics => _knowledgeStructureTopics.AsReadOnly();

    public void Update(string newName, string? description)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("New topic name cannot be empty.", nameof(newName));
        }

        Name = newName.Trim();
        Description = description?.Trim();
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("New topic name cannot be empty.", nameof(newName));
        }

        Name = newName.Trim();
    }
}
