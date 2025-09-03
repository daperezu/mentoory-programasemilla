using LinaSys.KnowledgeStructure.Domain.Aggregates.Topic;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Repositories;

public interface ITopicRepository : IRepository<Topic>
{
    Topic Add(Topic topic);

    Task<bool> IsNameTakenAsync(string name, long? excludingId = null, CancellationToken cancellationToken = default);

    Task<List<Topic>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<Topic?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
