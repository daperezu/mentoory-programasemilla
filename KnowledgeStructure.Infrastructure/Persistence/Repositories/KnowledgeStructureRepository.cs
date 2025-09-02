using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.KnowledgeStructure.Infrastructure.Persistence.Repositories;

public sealed class KnowledgeStructureRepository(KnowledgeStructureDbContext dbContext)
    : AbstractRepository<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure>(dbContext), IKnowledgeStructureRepository
{
    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.KnowledgeStructures
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure?> GetWithModulesTopicsAndSubjectsByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.KnowledgeStructures
            .AsNoTracking()
            .Include(x => x.KnowledgeStructureModules)
                .ThenInclude(m => m.Module)
            .Include(x => x.KnowledgeStructureModules)
                .ThenInclude(m => m.KnowledgeStructureTopics)
                    .ThenInclude(t => t.Topic)
            .Include(x => x.KnowledgeStructureModules)
                .ThenInclude(m => m.KnowledgeStructureTopics)
                    .ThenInclude(t => t.SubjectReferences)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure?> GetWithModulesAndTopicsByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.KnowledgeStructures
            .AsNoTracking()
            .Include(x => x.KnowledgeStructureModules)
                .ThenInclude(m => m.KnowledgeStructureTopics)
                    .ThenInclude(t => t.Topic)
            .Include(x => x.KnowledgeStructureModules)
                .ThenInclude(m => m.Module)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> IsNameTakenAsync(string name, long? excludingId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.KnowledgeStructures.AsQueryable();

        if (excludingId.HasValue)
        {
            query = query.Where(x => x.Id != excludingId.Value);
        }

        return query.AnyAsync(x => x.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure>> ListAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.KnowledgeStructures
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure>> GetAllWithModulesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.KnowledgeStructures
            .Include(x => x.KnowledgeStructureModules)
                .ThenInclude(m => m.Module)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule?> GetStructureModuleByIdAsync(long structureModuleId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule>()
            .Include(sm => sm.Module)
            .Include(sm => sm.KnowledgeStructure)
            .FirstOrDefaultAsync(sm => sm.Id == structureModuleId, cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule?> GetStructureModuleWithTopicsAsync(long structureModuleId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule>()
            .Include(sm => sm.Module)
            .Include(sm => sm.KnowledgeStructure)
            .Include(sm => sm.KnowledgeStructureTopics)
                .ThenInclude(st => st.Topic)
            .FirstOrDefaultAsync(sm => sm.Id == structureModuleId, cancellationToken);
    }

    public Task<List<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule>> GetAllStructureModulesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule>()
            .AsNoTracking()
            .Include(sm => sm.Module)
            .Include(sm => sm.KnowledgeStructure)
            .OrderBy(sm => sm.Order)
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic?> GetStructureTopicByIdAsync(long structureTopicId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>()
            .Include(st => st.Topic)
            .Include(st => st.KnowledgeStructureModule)
                .ThenInclude(m => m.Module)
            .Include(st => st.KnowledgeStructureModule)
                .ThenInclude(m => m.KnowledgeStructure)
            .Include(st => st.SubjectReferences)
            .FirstOrDefaultAsync(st => st.Id == structureTopicId, cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic?> GetStructureTopicWithSubjectsAsync(long structureTopicId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>()
            .Include(st => st.Topic)
            .Include(st => st.KnowledgeStructureModule)
                .ThenInclude(m => m.Module)
            .Include(st => st.KnowledgeStructureModule)
                .ThenInclude(m => m.KnowledgeStructure)
            .Include(st => st.SubjectReferences)
            .FirstOrDefaultAsync(st => st.Id == structureTopicId, cancellationToken);
    }

    public async Task DeleteStructureTopicAsync(Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic structureTopic, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>().Remove(structureTopic);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>> GetAllStructureTopicsAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>()
            .AsNoTracking()
            .Include(st => st.Topic)
            .Include(st => st.KnowledgeStructureModule)
                .ThenInclude(m => m.Module)
            .Include(st => st.KnowledgeStructureModule)
                .ThenInclude(m => m.KnowledgeStructure)
            .OrderBy(st => st.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateKnowledgeStructureAsync(Domain.Aggregates.KnowledgeStructure.KnowledgeStructure entity, CancellationToken cancellationToken = default)
    {
        Update(entity);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateModuleAsync(Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule>().Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateTopicAsync(Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>().Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>> GetTopicsReferencingSubjectAsync(long subjectId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>()
            .Include(st => st.Topic)
            .Include(st => st.SubjectReferences)
            .Where(st => st.SubjectReferences.Any(sr => sr.SubjectId == subjectId))
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure?> GetWithModulesAsync(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.KnowledgeStructures
            .Include(x => x.KnowledgeStructureModules)
                .ThenInclude(m => m.Module)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule?> GetModuleWithTopicsAsync(long moduleId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule>()
            .Include(m => m.Module)
            .Include(m => m.KnowledgeStructureTopics)
                .ThenInclude(t => t.Topic)
            .FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule?> GetModuleWithStructureAsync(long moduleId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule>()
            .Include(m => m.Module)
            .Include(m => m.KnowledgeStructure)
            .Include(m => m.KnowledgeStructureTopics)
            .FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic?> GetTopicWithSubjectReferencesAsync(long topicId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>()
            .Include(t => t.Topic)
            .Include(t => t.SubjectReferences)
            .FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken);
    }

    public Task<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic?> GetTopicWithModuleAsync(long topicId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>()
            .Include(t => t.Topic)
            .Include(t => t.KnowledgeStructureModule)
                .ThenInclude(m => m.Module)
            .FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken);
    }
}
