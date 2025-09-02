using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;

public class KnowledgeStructureModule : Entity
{
    private readonly List<KnowledgeStructureTopic> _knowledgeStructureTopics = [];

    public KnowledgeStructureModule(KnowledgeStructure structure, Module.Module module, int order)
    {
        KnowledgeStructure = structure ?? throw new ArgumentNullException(nameof(structure));
        KnowledgeStructureId = structure.Id;

        Module = module ?? throw new ArgumentNullException(nameof(module));
        ModuleId = module.Id;

        Order = order;
    }

    private KnowledgeStructureModule()
    {
    }

    public long KnowledgeStructureId { get; private set; }

    public long ModuleId { get; private set; }

    public int Order { get; private set; }

    public virtual KnowledgeStructure KnowledgeStructure { get; private set; }

    public IReadOnlyCollection<KnowledgeStructureTopic> KnowledgeStructureTopics => _knowledgeStructureTopics.AsReadOnly();

    public virtual Module.Module Module { get; private set; }

    public void AddTopic(Topic.Topic topic, int order)
    {
        var structureTopic = new KnowledgeStructureTopic(this, topic, order);
        _knowledgeStructureTopics.Add(structureTopic);
    }

    public void RemoveTopic(long topicId)
    {
        var topic = _knowledgeStructureTopics.FirstOrDefault(f => f.Id == topicId);
        if (topic is not null)
        {
            _knowledgeStructureTopics.Remove(topic);
        }
    }

    public void Reorder(int newOrder) => Order = newOrder;
}
