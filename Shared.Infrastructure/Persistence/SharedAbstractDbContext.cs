using System.Data;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LinaSys.Shared.Infrastructure.Persistence;

public abstract class SharedAbstractDbContext(DbContextOptions options, IMediator mediator) : DbContext(options), IDbContext, IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction is not null;

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction != _currentTransaction)
        {
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not the one in the current scope");
        }

        try
        {
            await SaveEntitiesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public IExecutionStrategy CreateExecutionStrategy()
    {
        return Database.CreateExecutionStrategy();
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await mediator.DispatchDomainEventsAsync(this);
        _ = await this.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IDbContextTransaction> TryBeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _currentTransaction ??= await Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken: cancellationToken);
    }
}
