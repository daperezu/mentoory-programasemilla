using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Domain.Aggregates.ProtectedResource;

public class UserProtectedResourcePermission : AuditableEntity
{
    private UserProtectedResourcePermission()
    {
    }

    public string UserId { get; private set; }

    public long ProtectedResourceId { get; private set; }

    public virtual ProtectedResource ProtectedResource { get; private set; }

    public static UserProtectedResourcePermission Create(
        string userId,
        long protectedResourceId,
        IAuditContext auditContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (protectedResourceId <= 0)
        {
            throw new ArgumentException("ProtectedResourceId must be greater than 0", nameof(protectedResourceId));
        }

        var permission = new UserProtectedResourcePermission
        {
            UserId = userId,
            ProtectedResourceId = protectedResourceId,
        };

        permission.SetCreated(auditContext);

        return permission;
    }
}
