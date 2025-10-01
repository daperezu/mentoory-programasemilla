using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Auth.Domain.AggregatesModel.Access;

/// <summary>
/// Read model for tracking user access to incubators.
/// Synchronized via integration events from BusinessIncubator domain.
/// </summary>
public class UserIncubatorAccess : Entity
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Gets the incubator identifier.
    /// </summary>
    public long IncubatorId { get; private set; }

    /// <summary>
    /// Gets the user's role in the incubator.
    /// </summary>
    public string Role { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the access is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the timestamp of the last synchronization.
    /// </summary>
    public DateTime LastSyncedAt { get; private set; }

    /// <summary>
    /// Creates a new instance of UserIncubatorAccess.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="incubatorId">The incubator identifier.</param>
    /// <param name="role">The user's role in the incubator.</param>
    /// <param name="syncedAt">The synchronization timestamp.</param>
    /// <returns>A new UserIncubatorAccess instance.</returns>
    public static UserIncubatorAccess Create(
        string userId,
        long incubatorId,
        string role,
        DateTime syncedAt)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("El identificador del usuario no puede estar vacío.", nameof(userId));
        }

        if (incubatorId <= 0)
        {
            throw new ArgumentException("El identificador de la incubadora debe ser mayor que cero.", nameof(incubatorId));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("El rol no puede estar vacío.", nameof(role));
        }

        return new UserIncubatorAccess
        {
            UserId = userId,
            IncubatorId = incubatorId,
            Role = role,
            IsActive = true,
            LastSyncedAt = syncedAt,
        };
    }

    /// <summary>
    /// Deactivates the user's access to the incubator.
    /// </summary>
    /// <param name="deactivatedAt">The deactivation timestamp.</param>
    public void Deactivate(DateTime deactivatedAt)
    {
        IsActive = false;
        LastSyncedAt = deactivatedAt;
    }

    /// <summary>
    /// Reactivates the user's access with an updated role.
    /// </summary>
    /// <param name="role">The new role for the user.</param>
    /// <param name="reactivatedAt">The reactivation timestamp.</param>
    public void Reactivate(string role, DateTime reactivatedAt)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("El rol no puede estar vacío.", nameof(role));
        }

        Role = role;
        IsActive = true;
        LastSyncedAt = reactivatedAt;
    }

    /// <summary>
    /// Updates the user's role in the incubator.
    /// </summary>
    /// <param name="role">The new role.</param>
    /// <param name="updatedAt">The update timestamp.</param>
    public void UpdateRole(string role, DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("El rol no puede estar vacío.", nameof(role));
        }

        Role = role;
        LastSyncedAt = updatedAt;
    }
}