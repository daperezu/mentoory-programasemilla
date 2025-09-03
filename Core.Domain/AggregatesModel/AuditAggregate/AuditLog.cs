#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1204 // Static elements should appear before instance elements

using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.AggregatesModel.AuditAggregate;

/// <summary>
/// Represents an audit log entry in the system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuditLog"/> class.
/// </remarks>
public class AuditLog(
    string entityType,
    string entityId,
    string action,
    string? userId = null,
    string? userName = null,
    Dictionary<string, object>? oldValues = null,
    Dictionary<string, object>? newValues = null,
    string? ipAddress = null,
    string? userAgent = null,
    string? additionalData = null) : Entity, IAggregateRoot
{
    /// <summary>
    /// Creates an audit log for a creation action.
    /// </summary>
    /// <returns></returns>
    public static AuditLog CreateForInsert(
        string entityType,
        string entityId,
        string userId,
        string userName,
        Dictionary<string, object> newValues,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new AuditLog(
            entityType,
            entityId,
            AuditActions.Create,
            userId,
            userName,
            null,
            newValues,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// Creates an audit log for an update action.
    /// </summary>
    /// <returns></returns>
    public static AuditLog CreateForUpdate(
        string entityType,
        string entityId,
        string userId,
        string userName,
        Dictionary<string, object> oldValues,
        Dictionary<string, object> newValues,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new AuditLog(
            entityType,
            entityId,
            AuditActions.Update,
            userId,
            userName,
            oldValues,
            newValues,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// Creates an audit log for a deletion action.
    /// </summary>
    /// <returns></returns>
    public static AuditLog CreateForDelete(
        string entityType,
        string entityId,
        string userId,
        string userName,
        Dictionary<string, object> oldValues,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new AuditLog(
            entityType,
            entityId,
            AuditActions.Delete,
            userId,
            userName,
            oldValues,
            null,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// Gets the type of entity being audited.
    /// </summary>
    public string EntityType { get; private set; } = entityType;

    /// <summary>
    /// Gets the ID of the entity being audited.
    /// </summary>
    public string EntityId { get; private set; } = entityId;

    /// <summary>
    /// Gets the action performed (e.g., Create, Update, Delete).
    /// </summary>
    public string Action { get; private set; } = action;

    /// <summary>
    /// Gets the ID of the user who performed the action.
    /// </summary>
    public string? UserId { get; private set; } = userId;

    /// <summary>
    /// Gets the name of the user who performed the action.
    /// </summary>
    public string? UserName { get; private set; } = userName;

    /// <summary>
    /// Gets the timestamp when the action occurred.
    /// </summary>
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the old values before the change (for updates).
    /// </summary>
    public Dictionary<string, object>? OldValues { get; private set; } = oldValues;

    /// <summary>
    /// Gets the new values after the change (for updates).
    /// </summary>
    public Dictionary<string, object>? NewValues { get; private set; } = newValues;

    /// <summary>
    /// Gets the IP address from which the action was performed.
    /// </summary>
    public string? IpAddress { get; private set; } = ipAddress;

    /// <summary>
    /// Gets the user agent of the client.
    /// </summary>
    public string? UserAgent { get; private set; } = userAgent;

    /// <summary>
    /// Gets additional data related to the audit entry.
    /// </summary>
    public string? AdditionalData { get; private set; } = additionalData;
}

/// <summary>
/// Standard audit action types.
/// </summary>
public static class AuditActions
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string View = "View";
    public const string Export = "Export";
    public const string Import = "Import";
    public const string Approve = "Approve";
    public const string Reject = "Reject";
    public const string Submit = "Submit";
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string PasswordReset = "PasswordReset";
    public const string RoleAssignment = "RoleAssignment";
    public const string PermissionGrant = "PermissionGrant";
    public const string PermissionRevoke = "PermissionRevoke";
}
