using LinaSys.Shared.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Shared.Infrastructure.Persistence.Repositories;

/// <summary>
/// Abstract base class for repository implementations.
/// </summary>
/// <typeparam name="T">The type of the aggregate root.</typeparam>
public abstract class AbstractRepository<T>(DbContext context)
    : IRepository<T>
    where T : class, IAggregateRoot
{
    /// <summary>
    /// Gets the unit of work associated with the repository.
    /// </summary>
    public IUnitOfWork UnitOfWork => (IUnitOfWork)context;

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to add.</param>
    /// <returns>The added aggregate.</returns>
    public T Add(T aggregate)
    {
        return context.Set<T>().Add(aggregate).Entity;
    }

    public ValueTask<T?> FindByIdAsync(long id, CancellationToken cancellationToken)
    {
        return context.Set<T>().FindAsync([id], cancellationToken);
    }

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to update.</param>
    public void Update(T aggregate)
    {
        // Only attach if it's not tracked
        if (context.Entry(aggregate).State == EntityState.Detached)
        {
            context.Update(aggregate);
        }
    }
}
