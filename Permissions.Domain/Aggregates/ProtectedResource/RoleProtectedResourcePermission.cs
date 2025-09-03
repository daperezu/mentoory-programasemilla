using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Domain.Aggregates.ProtectedResource;

public class RoleProtectedResourcePermission : AuditableEntity
{
    private RoleProtectedResourcePermission()
    {
    }

    public string Role { get; private set; }

    public long ProtectedResourceId { get; private set; }

    public virtual ProtectedResource ProtectedResource { get; private set; }

    public static RoleProtectedResourcePermission Create(
        string role,
        long protectedResourceId,
        IAuditContext auditContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        if (protectedResourceId <= 0)
        {
            throw new ArgumentException("ProtectedResourceId must be greater than 0", nameof(protectedResourceId));
        }

        var permission = new RoleProtectedResourcePermission
        {
            Role = role,
            ProtectedResourceId = protectedResourceId,
        };

        permission.SetCreated(auditContext);

        return permission;
    }
}
