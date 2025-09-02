using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;

public class KnowledgeStructureTopic : Entity
{
    private readonly List<SubjectReference> _subjectReferences = [];

    public KnowledgeStructureTopic(KnowledgeStructureModule module, Topic.Topic topic, int order)
    {
        KnowledgeStructureModule = module ?? throw new ArgumentNullException(nameof(module));
        KnowledgeStructureModuleId = module.Id;

        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        TopicId = topic.Id;

        Order = order;
    }

    private KnowledgeStructureTopic()
    {
    }

    public long KnowledgeStructureModuleId { get; private set; }

    public long TopicId { get; private set; }

    public int Order { get; private set; }

    public virtual KnowledgeStructureModule KnowledgeStructureModule { get; private set; }

    public virtual Topic.Topic Topic { get; private set; }

    public IReadOnlyCollection<SubjectReference> SubjectReferences => _subjectReferences.AsReadOnly();

    public void AddSubjectReference(long subjectId, int? order = null)
    {
        if (_subjectReferences.Any(sr => sr.SubjectId == subjectId))
        {
            throw new InvalidOperationException($"Subject {subjectId} is already referenced in this topic.");
        }

        var actualOrder = order ?? (_subjectReferences.Count > 0 ? _subjectReferences.Max(sr => sr.Order) + 1 : 1);
        var reference = new SubjectReference(subjectId, actualOrder);
        _subjectReferences.Add(reference);
    }

    public void RemoveSubjectReference(long subjectId)
    {
        var reference = _subjectReferences.FirstOrDefault(sr => sr.SubjectId == subjectId);
        if (reference is not null)
        {
            _subjectReferences.Remove(reference);
            ReorderSubjectReferences();
        }
    }

    public void Reorder(int newOrder) => Order = newOrder;

    private void ReorderSubjectReferences()
    {
        var orderedReferences = _subjectReferences.OrderBy(sr => sr.Order).ToList();
        _subjectReferences.Clear();

        for (int i = 0; i < orderedReferences.Count; i++)
        {
            _subjectReferences.Add(new SubjectReference(orderedReferences[i].SubjectId, i + 1));
        }
    }
}
