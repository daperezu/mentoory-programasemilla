using LinaSys.Core.Domain.AggregatesModel.AuditAggregate;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Core.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for audit logs.
/// </summary>
public class AuditLogRepository(CoreDbContext context) : IAuditLogRepository
{
    private readonly CoreDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public IUnitOfWork UnitOfWork => _context;

    public void Add(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public IQueryable<AuditLog> GetQueryable()
    {
        return _context.AuditLogs.AsQueryable();
    }

    public async Task<int> CountAsync(IQueryable<AuditLog> query)
    {
        return await query.CountAsync();
    }

    public async Task<List<AuditLog>> GetPagedAsync(IQueryable<AuditLog> query, int skip, int take)
    {
        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctEntityTypesAsync()
    {
        return await _context.AuditLogs
            .Select(a => a.EntityType)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctActionsAsync()
    {
        return await _context.AuditLogs
            .Select(a => a.Action)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();
    }
}
