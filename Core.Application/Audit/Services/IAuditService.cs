namespace LinaSys.Core.Application.Audit.Services;

/// <summary>
/// Service interface for audit logging.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an entity creation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogCreateAsync<TEntity>(
        TEntity entity,
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null)
        where TEntity : class;

    /// <summary>
    /// Logs an entity update.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogUpdateAsync<TEntity>(
        TEntity oldEntity,
        TEntity newEntity,
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null)
        where TEntity : class;

    /// <summary>
    /// Logs an entity deletion.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogDeleteAsync<TEntity>(
        TEntity entity,
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null)
        where TEntity : class;

    /// <summary>
    /// Logs a custom action.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogActionAsync(
        string entityType,
        string entityId,
        string action,
        string userId,
        string userName,
        Dictionary<string, object>? oldValues = null,
        Dictionary<string, object>? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalData = null);

    /// <summary>
    /// Logs a user authentication event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogAuthenticationAsync(
        string userId,
        string userName,
        string action,
        bool success,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalData = null);
}
