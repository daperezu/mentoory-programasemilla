using System.Diagnostics.CodeAnalysis;

namespace LinaSys.Shared.Infrastructure.Persistence;

public interface IDbContextFactory
{
    bool TryGetDbContextForRequest<TRequest>([NotNullWhen(true)] out IDbContext? dbContext);
}
