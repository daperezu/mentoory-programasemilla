using System.Data;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Shared.Infrastructure;
using LinaSys.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LinaSys.Auth.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the authentication module.
/// It provides methods for transaction management and execution strategy.
/// Implements the <see cref="IUnitOfWork"/> and <see cref="IDbContext"/> interfaces just like <see cref="SharedAbstractDbContext"/> but since we inherit IdentityDbContext, we need to implement the methods here.
/// This is an exception to the rule of reuse the <see cref="SharedAbstractDbContext"/>.
/// </summary>
public partial class AuthDbContext : IUnitOfWork, IDbContext
{
    private IDbContextTransaction? _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction is not null;

    public async Task<IDbContextTransaction> TryBeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _currentTransaction ??= await Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken: cancellationToken);
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction != _currentTransaction)
        {
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);
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
}
