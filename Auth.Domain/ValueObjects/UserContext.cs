using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Auth.Domain.ValueObjects;

/// <summary>
/// Value object representing the user's current working context (role, incubator, project).
/// </summary>
public class UserContext : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserContext"/> class.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The selected role identifier.</param>
    /// <param name="incubatorId">The selected business incubator identifier (optional).</param>
    /// <param name="projectId">The selected project identifier (optional).</param>
    /// <param name="isGlobalAdministrator">Indicates if user is a global administrator.</param>
    private UserContext(
        string userId,
        string? role,
        long? incubatorId,
        long? projectId,
        bool isGlobalAdministrator)
    {
        UserId = userId;
        Role = role;
        IncubatorId = incubatorId;
        ProjectId = projectId;
        IsGlobalAdministrator = isGlobalAdministrator;
    }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Gets the selected role identifier.
    /// </summary>
    public string? Role { get; private set; }

    /// <summary>
    /// Gets the selected business incubator identifier.
    /// </summary>
    public long? IncubatorId { get; private set; }

    /// <summary>
    /// Gets the selected project identifier.
    /// </summary>
    public long? ProjectId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user is a global administrator.
    /// Global administrators can access any incubator/project.
    /// </summary>
    public bool IsGlobalAdministrator { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the context is complete (all required selections made).
    /// </summary>
    public bool IsComplete => !string.IsNullOrEmpty(Role) &&
                             (IsGlobalAdministrator || (IncubatorId.HasValue && ProjectId.HasValue));

    /// <summary>
    /// Creates a new UserContext for a global administrator.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role identifier.</param>
    /// <param name="incubatorId">The selected incubator (optional for global admin).</param>
    /// <param name="projectId">The selected project (optional for global admin).</param>
    /// <returns>A new UserContext instance.</returns>
    public static UserContext CreateForGlobalAdministrator(
        string userId,
        string role,
        long? incubatorId = null,
        long? projectId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role ID cannot be empty.", nameof(role));
        }

        return new UserContext(userId, role, incubatorId, projectId, true);
    }

    /// <summary>
    /// Creates a new UserContext for a coordinator.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role identifier.</param>
    /// <param name="incubatorId">The selected incubator (required).</param>
    /// <param name="projectId">The selected project (required).</param>
    /// <returns>A new UserContext instance.</returns>
    public static UserContext CreateForUser(
        string userId,
        string role,
        long incubatorId,
        long projectId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be empty.", nameof(role));
        }

        if (incubatorId <= 0)
        {
            throw new ArgumentException("Incubator ID must be a positive value.", nameof(incubatorId));
        }

        if (projectId <= 0)
        {
            throw new ArgumentException("Project ID must be a positive value.", nameof(projectId));
        }

        return new UserContext(userId, role, incubatorId, projectId, false);
    }

    /// <summary>
    /// Creates an empty UserContext (no selections made).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new empty UserContext instance.</returns>
    public static UserContext CreateEmpty(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        return new UserContext(userId, null, null, null, false);
    }

    /// <summary>
    /// Updates the context with a new role selection.
    /// </summary>
    /// <param name="role">The new role identifier.</param>
    /// <returns>A new UserContext with the updated role.</returns>
    public UserContext WithRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be empty.", nameof(role));
        }

        return new UserContext(UserId, role, IncubatorId, ProjectId, IsGlobalAdministrator);
    }

    /// <summary>
    /// Updates the context with a new incubator selection.
    /// </summary>
    /// <param name="incubatorId">The new incubator identifier.</param>
    /// <returns>A new UserContext with the updated incubator.</returns>
    public UserContext WithIncubator(long incubatorId)
    {
        if (incubatorId <= 0)
        {
            throw new ArgumentException("Incubator ID must be a positive value.", nameof(incubatorId));
        }

        return new UserContext(UserId, Role, incubatorId, ProjectId, IsGlobalAdministrator);
    }

    /// <summary>
    /// Updates the context with a new project selection.
    /// </summary>
    /// <param name="projectId">The new project identifier.</param>
    /// <returns>A new UserContext with the updated project.</returns>
    public UserContext WithProject(long projectId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Project ID must be a positive value.", nameof(projectId));
        }

        return new UserContext(UserId, Role, IncubatorId, projectId, IsGlobalAdministrator);
    }

    /// <summary>
    /// Clears the context selections (keeps only user ID).
    /// </summary>
    /// <returns>A new empty UserContext.</returns>
    public UserContext Clear()
    {
        return CreateEmpty(UserId);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return UserId;
        yield return Role;
        yield return IncubatorId;
        yield return ProjectId;
        yield return IsGlobalAdministrator;
    }
}
