using LinaSys.KnowledgeStructure.Domain.Aggregates.Module;
using LinaSys.KnowledgeStructure.Domain.Aggregates.Topic;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Repositories;

public interface IModuleRepository : IRepository<Module>
{
    Module Add(Module module);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

    Task<List<Module>> GetAllModulesWithTopicsAsync(CancellationToken cancellationToken);

    Task<List<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken);

    Task<Module?> GetByIdWithTopicsAsync(long requestModuleId);

    Task<Module?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Module?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task<Topic?> GetTopicByNameInModuleAsync(Module module, string topicName, CancellationToken cancellationToken);

    void Update(Module module);

    Task<bool> IsNameTakenAsync(string name, long? excludingId = null, CancellationToken cancellationToken = default);

    Task<List<Module>> ListAllAsync(CancellationToken cancellationToken = default);
}
