using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;

public class KnowledgeStructureSubject : Entity
{
    public KnowledgeStructureSubject(KnowledgeStructureTopic topic, Subject.Subject subject, int order)
    {
        KnowledgeStructureTopic = topic ?? throw new ArgumentNullException(nameof(topic));
        KnowledgeStructureTopicId = topic.Id;

        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        SubjectId = subject.Id;

        Order = order;
    }

    private KnowledgeStructureSubject()
    {
    }

    public long KnowledgeStructureTopicId { get; set; }

    public long SubjectId { get; set; }

    public int Order { get; set; }

    public virtual KnowledgeStructureTopic KnowledgeStructureTopic { get; set; }

    public virtual Subject.Subject Subject { get; set; }

    public void Reorder(int newOrder)
    {
        Order = newOrder;
    }
}
