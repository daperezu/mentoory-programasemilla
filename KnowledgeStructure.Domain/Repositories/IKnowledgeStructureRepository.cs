using LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Repositories;

public interface IKnowledgeStructureRepository : IRepository<Aggregates.KnowledgeStructure.KnowledgeStructure>
{
    Aggregates.KnowledgeStructure.KnowledgeStructure Add(Aggregates.KnowledgeStructure.KnowledgeStructure entity);

    ValueTask<Aggregates.KnowledgeStructure.KnowledgeStructure?> FindByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Aggregates.KnowledgeStructure.KnowledgeStructure?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    void Update(Aggregates.KnowledgeStructure.KnowledgeStructure entity);

    Task<Aggregates.KnowledgeStructure.KnowledgeStructure?> GetWithModulesTopicsAndSubjectsByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Aggregates.KnowledgeStructure.KnowledgeStructure?> GetWithModulesAndTopicsByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> IsNameTakenAsync(string name, long? excludingId = null, CancellationToken cancellationToken = default);

    Task<List<Aggregates.KnowledgeStructure.KnowledgeStructure>> ListAllActiveAsync(CancellationToken cancellationToken = default);

    Task<List<Aggregates.KnowledgeStructure.KnowledgeStructure>> GetAllWithModulesAsync(CancellationToken cancellationToken = default);

    // Module-specific methods
    Task<KnowledgeStructureModule?> GetStructureModuleByIdAsync(long structureModuleId, CancellationToken cancellationToken = default);

    Task<KnowledgeStructureModule?> GetStructureModuleWithTopicsAsync(long structureModuleId, CancellationToken cancellationToken = default);

    Task<List<KnowledgeStructureModule>> GetAllStructureModulesAsync(CancellationToken cancellationToken = default);

    // Topic-specific methods
    Task<KnowledgeStructureTopic?> GetStructureTopicByIdAsync(long structureTopicId, CancellationToken cancellationToken = default);

    Task<KnowledgeStructureTopic?> GetStructureTopicWithSubjectsAsync(long structureTopicId, CancellationToken cancellationToken = default);

    Task DeleteStructureTopicAsync(KnowledgeStructureTopic structureTopic, CancellationToken cancellationToken = default);

    Task<List<KnowledgeStructureTopic>> GetAllStructureTopicsAsync(CancellationToken cancellationToken = default);

    Task UpdateKnowledgeStructureAsync(Aggregates.KnowledgeStructure.KnowledgeStructure entity, CancellationToken cancellationToken = default);

    Task UpdateModuleAsync(KnowledgeStructureModule entity, CancellationToken cancellationToken = default);

    Task UpdateTopicAsync(KnowledgeStructureTopic entity, CancellationToken cancellationToken = default);

    // Subject reference methods
    Task<List<KnowledgeStructureTopic>> GetTopicsReferencingSubjectAsync(long subjectId, CancellationToken cancellationToken = default);

    // Additional query methods for Knowledge Structure Builder
    Task<Aggregates.KnowledgeStructure.KnowledgeStructure?> GetWithModulesAsync(long id, CancellationToken cancellationToken = default);

    Task<KnowledgeStructureModule?> GetModuleWithTopicsAsync(long moduleId, CancellationToken cancellationToken = default);

    Task<KnowledgeStructureModule?> GetModuleWithStructureAsync(long moduleId, CancellationToken cancellationToken = default);

    Task<KnowledgeStructureTopic?> GetTopicWithSubjectReferencesAsync(long topicId, CancellationToken cancellationToken = default);

    Task<KnowledgeStructureTopic?> GetTopicWithModuleAsync(long topicId, CancellationToken cancellationToken = default);
}
