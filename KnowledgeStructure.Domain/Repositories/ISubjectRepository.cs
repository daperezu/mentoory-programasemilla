using LinaSys.KnowledgeStructure.Domain.Aggregates.Subject;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Repositories;

public interface ISubjectRepository : IRepository<Subject>
{
    Task<bool> IsTitleTakenAsync(string title, long? excludingId = null, CancellationToken cancellationToken = default);

    Task<List<Subject>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<Subject?> GetWithResourcesByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<List<Subject>> GetAllWithResourcesAsync(CancellationToken cancellationToken = default);

    ValueTask<Subject?> FindByIdAsync(long id, CancellationToken cancellationToken = default);

    Subject Add(Subject subject);

    void Remove(Subject subject);

    Task<Subject?> GetWithResourcesAsync(long subjectId, CancellationToken cancellationToken = default);

    Task<List<Subject>> GetByIdsAsync(List<long> ids, CancellationToken cancellationToken = default);
}
