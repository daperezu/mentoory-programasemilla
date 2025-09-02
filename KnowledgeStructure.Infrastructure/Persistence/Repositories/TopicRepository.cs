using LinaSys.KnowledgeStructure.Domain.Aggregates.Topic;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.KnowledgeStructure.Infrastructure.Persistence.Repositories;

public sealed class TopicRepository(KnowledgeStructureDbContext dbContext)
    : AbstractRepository<Topic>(dbContext), ITopicRepository
{
    public Task<bool> IsNameTakenAsync(string name, long? excludingId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Topics.AsQueryable();

        if (excludingId.HasValue)
        {
            query = query.Where(x => x.Id != excludingId.Value);
        }

        return query.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public Task<List<Topic>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Topics
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Topic?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.Topics
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
