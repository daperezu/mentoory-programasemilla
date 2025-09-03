using LinaSys.KnowledgeStructure.Domain.Aggregates.Module;
using LinaSys.KnowledgeStructure.Domain.Aggregates.Topic;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.KnowledgeStructure.Infrastructure.Persistence.Repositories;

public class ModuleRepository(KnowledgeStructureDbContext dbContext)
    : AbstractRepository<Module>(dbContext), IModuleRepository
{
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return dbContext.Modules
            .AsNoTracking()
            .AnyAsync(x => x.Name == name, cancellationToken);
    }

    public Task<List<Module>> GetAllModulesWithTopicsAsync(CancellationToken cancellationToken)
    {
        return dbContext.Modules
            .AsNoTracking()
            ////.Include(i => i.Topics)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken)
    {
        return dbContext.Topics
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Module?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.Modules
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Module?> GetByIdWithTopicsAsync(long requestModuleId)
    {
        return dbContext.Modules
            .AsNoTracking()
            ////.Include(x => x.Topics)
            .FirstOrDefaultAsync(x => x.Id == requestModuleId);
    }

    public Task<Module?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return dbContext.Modules
            ////.Include(i => i.Topics)
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public Task<Topic?> GetTopicByNameInModuleAsync(Module module, string topicName, CancellationToken cancellationToken)
    {
        return dbContext.Topics
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == topicName /*&& x.Module.Id == module.Id*/, cancellationToken);
    }

    public Task<bool> IsNameTakenAsync(string name, long? excludingId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Modules.AsQueryable();

        if (excludingId.HasValue)
        {
            query = query.Where(x => x.Id != excludingId.Value);
        }

        return query.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public Task<List<Module>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Modules
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<Module> GetQueryable()
    {
        return dbContext.Modules.AsNoTracking();
    }
}
