using System.Reflection;
using System.Text.Json;
using LinaSys.Core.Application.Audit.Services;
using LinaSys.Core.Domain.AggregatesModel.AuditAggregate;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Infrastructure.Services;

/// <summary>
/// Service implementation for audit logging.
/// </summary>
public class AuditService(
    IAuditLogRepository auditRepository,
    IUnitOfWork unitOfWork) : IAuditService
{
    public async Task LogCreateAsync<TEntity>(
        TEntity entity,
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null)
        where TEntity : class
    {
        var entityType = typeof(TEntity).Name;
        var entityId = GetEntityId(entity);
        var values = GetEntityValues(entity);

        var auditLog = AuditLog.CreateForInsert(
            entityType,
            entityId,
            userId,
            userName,
            values,
            ipAddress,
            userAgent);

        auditRepository.Add(auditLog);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task LogUpdateAsync<TEntity>(
        TEntity oldEntity,
        TEntity newEntity,
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null)
        where TEntity : class
    {
        var entityType = typeof(TEntity).Name;
        var entityId = GetEntityId(newEntity);
        var oldValues = GetEntityValues(oldEntity);
        var newValues = GetEntityValues(newEntity);

        // Only log if there are actual changes
        if (!ValuesAreEqual(oldValues, newValues))
        {
            var auditLog = AuditLog.CreateForUpdate(
                entityType,
                entityId,
                userId,
                userName,
                oldValues,
                newValues,
                ipAddress,
                userAgent);

            auditRepository.Add(auditLog);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public async Task LogDeleteAsync<TEntity>(
        TEntity entity,
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null)
        where TEntity : class
    {
        var entityType = typeof(TEntity).Name;
        var entityId = GetEntityId(entity);
        var values = GetEntityValues(entity);

        var auditLog = AuditLog.CreateForDelete(
            entityType,
            entityId,
            userId,
            userName,
            values,
            ipAddress,
            userAgent);

        auditRepository.Add(auditLog);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task LogActionAsync(
        string entityType,
        string entityId,
        string action,
        string userId,
        string userName,
        Dictionary<string, object>? oldValues = null,
        Dictionary<string, object>? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalData = null)
    {
        var auditLog = new AuditLog(
            entityType,
            entityId,
            action,
            userId,
            userName,
            oldValues,
            newValues,
            ipAddress,
            userAgent,
            additionalData);

        auditRepository.Add(auditLog);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task LogAuthenticationAsync(
        string userId,
        string userName,
        string action,
        bool success,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalData = null)
    {
        var auditLog = new AuditLog(
            "Authentication",
            userId,
            action,
            userId,
            userName,
            null,
            new Dictionary<string, object> { { "Success", success } },
            ipAddress,
            userAgent,
            additionalData);

        auditRepository.Add(auditLog);
        await unitOfWork.SaveChangesAsync();
    }

    private string GetEntityId<TEntity>(TEntity entity)
        where TEntity : class
    {
        // Try to get Id property
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty != null)
        {
            var value = idProperty.GetValue(entity);
            return value?.ToString() ?? "Unknown";
        }

        // Try to get primary key for Entity Framework entities
        var keyProperty = entity.GetType().GetProperties()
            .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

        if (keyProperty != null)
        {
            var value = keyProperty.GetValue(entity);
            return value?.ToString() ?? "Unknown";
        }

        return "Unknown";
    }

    private Dictionary<string, object> GetEntityValues<TEntity>(TEntity entity)
        where TEntity : class
    {
        var values = new Dictionary<string, object>();
        var properties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip navigation properties and collections
            if (property.PropertyType.IsClass &&
                property.PropertyType != typeof(string) &&
                !property.PropertyType.IsArray &&
                !property.PropertyType.IsGenericType)
            {
                continue;
            }

            try
            {
                var value = property.GetValue(entity);
                if (value != null)
                {
                    // Handle special types
                    if (value is DateTime dateTime)
                    {
                        values[property.Name] = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        values[property.Name] = value.ToString()!;
                    }
                    else
                    {
                        values[property.Name] = value;
                    }
                }
            }
            catch
            {
                // Skip properties that can't be read
            }
        }

        return values;
    }

    private bool ValuesAreEqual(Dictionary<string, object> oldValues, Dictionary<string, object> newValues)
    {
        if (oldValues.Count != newValues.Count)
        {
            return false;
        }

        foreach (var kvp in oldValues)
        {
            if (!newValues.TryGetValue(kvp.Key, out var newValue))
            {
                return false;
            }

            var oldJson = JsonSerializer.Serialize(kvp.Value);
            var newJson = JsonSerializer.Serialize(newValue);

            if (oldJson != newJson)
            {
                return false;
            }
        }

        return true;
    }
}
