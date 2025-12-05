using LinaSys.Core.Domain.AggregatesModel.AuditAggregate;
using LinaSys.Shared.Application;
using MediatR;

namespace LinaSys.Core.Application.Audit.Queries;

/// <summary>
/// Query to retrieve audit logs with filtering options.
/// </summary>
public record GetAuditLogsQuery(
    string? EntityType = null,
    string? EntityId = null,
    string? UserId = null,
    string? Action = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 50) : LinaSys.Shared.Application.MediatR.IBaseRequest<AuditLogPagedResult>;

/// <summary>
/// Result containing paged audit logs.
/// </summary>
public record AuditLogPagedResult(
    List<AuditLogDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// DTO for audit log entries.
/// </summary>
public record AuditLogDto(
    long Id,
    string EntityType,
    string EntityId,
    string Action,
    string? UserId,
    string? UserName,
    DateTime Timestamp,
    Dictionary<string, object>? OldValues,
    Dictionary<string, object>? NewValues,
    string? IpAddress,
    string? UserAgent,
    string? AdditionalData);

/// <summary>
/// Handler for retrieving audit logs.
/// </summary>
public class GetAuditLogsQueryHandler(IAuditLogRepository repository) : IRequestHandler<GetAuditLogsQuery, Result<AuditLogPagedResult>>
{
    public async Task<Result<AuditLogPagedResult>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        if (!string.IsNullOrEmpty(request.EntityId))
        {
            query = query.Where(a => a.EntityId == request.EntityId);
        }

        if (!string.IsNullOrEmpty(request.UserId))
        {
            query = query.Where(a => a.UserId == request.UserId);
        }

        if (!string.IsNullOrEmpty(request.Action))
        {
            query = query.Where(a => a.Action == request.Action);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.EndDate.Value);
        }

        // Get total count
        var totalCount = await repository.CountAsync(query);

        // Apply pagination
        var skip = (request.PageNumber - 1) * request.PageSize;
        var logs = await repository.GetPagedAsync(query, skip, request.PageSize);

        // Map to DTOs
        var items = logs.Select(log => new AuditLogDto(
            log.Id,
            log.EntityType,
            log.EntityId,
            log.Action,
            log.UserId,
            log.UserName,
            log.Timestamp,
            log.OldValues,
            log.NewValues,
            log.IpAddress,
            log.UserAgent,
            log.AdditionalData)).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var result = new AuditLogPagedResult(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Success(result);
    }
}
