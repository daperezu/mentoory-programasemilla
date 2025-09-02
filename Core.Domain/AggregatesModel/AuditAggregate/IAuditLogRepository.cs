using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.AggregatesModel.AuditAggregate;

/// <summary>
/// Repository interface for audit logs.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Adds a new audit log entry.
    /// </summary>
    void Add(AuditLog auditLog);

    /// <summary>
    /// Gets audit logs for a specific entity.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByUserAsync(string userId);

    /// <summary>
    /// Gets a queryable for complex filtering.
    /// </summary>
    IQueryable<AuditLog> GetQueryable();

    /// <summary>
    /// Counts audit logs matching the query.
    /// </summary>
    Task<int> CountAsync(IQueryable<AuditLog> query);

    /// <summary>
    /// Gets paged audit logs.
    /// </summary>
    Task<List<AuditLog>> GetPagedAsync(IQueryable<AuditLog> query, int skip, int take);

    /// <summary>
    /// Gets audit logs within a date range.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets distinct entity types that have been audited.
    /// </summary>
    Task<IEnumerable<string>> GetDistinctEntityTypesAsync();

    /// <summary>
    /// Gets distinct actions that have been logged.
    /// </summary>
    Task<IEnumerable<string>> GetDistinctActionsAsync();
}