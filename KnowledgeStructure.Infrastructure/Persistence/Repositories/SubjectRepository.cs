using LinaSys.KnowledgeStructure.Domain.Aggregates.Subject;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.KnowledgeStructure.Infrastructure.Persistence.Repositories;

public sealed class SubjectRepository(KnowledgeStructureDbContext dbContext)
    : AbstractRepository<Subject>(dbContext), ISubjectRepository
{
    /// <inheritdoc />
    public Task<bool> IsTitleTakenAsync(string title, long? excludingId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Subjects.AsQueryable();

        if (excludingId.HasValue)
        {
            query = query.Where(x => x.Id != excludingId.Value);
        }

        return query.AnyAsync(x => x.Title == title, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<Subject>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Subjects
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Subject?> GetWithResourcesByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.Subjects
            .Include(s => s.SubjectResources)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<Subject>> GetAllWithResourcesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Subjects
            .Include(s => s.SubjectResources)
            .OrderBy(s => s.Title)
            .ToListAsync(cancellationToken);
    }

    public void Remove(Subject subject)
    {
        dbContext.Subjects.Remove(subject);
    }

    public Task<Subject?> GetWithResourcesAsync(long subjectId, CancellationToken cancellationToken = default)
    {
        return dbContext.Subjects
            .Include(s => s.SubjectResources)
            .FirstOrDefaultAsync(s => s.Id == subjectId, cancellationToken);
    }

    public Task<List<Subject>> GetByIdsAsync(List<long> ids, CancellationToken cancellationToken = default)
    {
        return dbContext.Subjects
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }
}
