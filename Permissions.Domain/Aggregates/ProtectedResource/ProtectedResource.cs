using LinaSys.Permissions.Domain.Constants;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Domain.Aggregates.ProtectedResource;

public class ProtectedResource : AuditableEntity, IAggregateRoot
{
    private ProtectedResource()
    {
    }

    public Guid ExternalId { get; private set; }

    public int ResourceType { get; private set; }

    public string Name { get; private set; }

    public virtual ICollection<RoleProtectedResourcePermission> RoleProtectedResourcePermissions { get; private set; } = [];

    public virtual ICollection<UserProtectedResourcePermission> UserProtectedResourcePermissions { get; private set; } = [];

    public static ProtectedResource Create(
        Guid externalId,
        int resourceType,
        string name,
        IAuditContext auditContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!ResourceTypes.IsValid(resourceType))
        {
            throw new ArgumentException($"Invalid resource type: {resourceType}", nameof(resourceType));
        }

        if (externalId == Guid.Empty)
        {
            throw new ArgumentException("ExternalId cannot be empty", nameof(externalId));
        }

        var protectedResource = new ProtectedResource
        {
            ExternalId = externalId,
            ResourceType = resourceType,
            Name = name.Trim(),
        };

        protectedResource.SetCreated(auditContext);

        return protectedResource;
    }

    public void UpdateName(string name, IAuditContext auditContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (Name != name.Trim())
        {
            Name = name.Trim();
            SetUpdated(auditContext);
        }
    }

    public void GrantAccessToRole(string role, IAuditContext auditContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        if (!RoleProtectedResourcePermissions.Any(r => r.Role == role))
        {
            var permission = RoleProtectedResourcePermission.Create(role, Id, auditContext);
            RoleProtectedResourcePermissions.Add(permission);
            SetUpdated(auditContext);
        }
    }

    public void RevokeAccessFromRole(string role, IAuditContext auditContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        var permission = RoleProtectedResourcePermissions.FirstOrDefault(r => r.Role == role);
        if (permission is not null)
        {
            RoleProtectedResourcePermissions.Remove(permission);
            SetUpdated(auditContext);
        }
    }

    public void GrantAccessToUser(string userId, IAuditContext auditContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (!UserProtectedResourcePermissions.Any(u => u.UserId == userId))
        {
            var permission = UserProtectedResourcePermission.Create(userId, Id, auditContext);
            UserProtectedResourcePermissions.Add(permission);
            SetUpdated(auditContext);
        }
    }

    public void RevokeAccessFromUser(string userId, IAuditContext auditContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var permission = UserProtectedResourcePermissions.FirstOrDefault(u => u.UserId == userId);
        if (permission is not null)
        {
            UserProtectedResourcePermissions.Remove(permission);
            SetUpdated(auditContext);
        }
    }

    public bool HasRoleAccess(string role)
    {
        return RoleProtectedResourcePermissions.Any(r => r.Role == role);
    }

    public bool HasUserAccess(string userId)
    {
        return UserProtectedResourcePermissions.Any(u => u.UserId == userId);
    }

    public string GetResourceTypeDisplayName()
    {
        return ResourceTypes.GetDisplayName(ResourceType);
    }
}
