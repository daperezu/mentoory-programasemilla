using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Partial class for managing project users within a project.
/// </summary>
public partial class Project
{
    private readonly List<ProjectUser> _projectUsers = [];

    /// <summary>
    /// Gets the read-only collection of project users.
    /// </summary>
    public IReadOnlyCollection<ProjectUser> ProjectUsers => _projectUsers.AsReadOnly();

    /// <summary>
    /// Gets all active users in the project.
    /// </summary>
    /// <returns>Collection of active project users.</returns>
    public IEnumerable<ProjectUser> GetActiveUsers()
    {
        return _projectUsers.Where(u => u.IsActive);
    }

    /// <summary>
    /// Gets users by role.
    /// </summary>
    /// <param name="role">The role to filter by.</param>
    /// <returns>Collection of project users with the specified role.</returns>
    public IEnumerable<ProjectUser> GetUsersByRole(string role)
    {
        return _projectUsers.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase) && u.IsActive);
    }

    /// <summary>
    /// Gets a specific user by their user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The project user if found.</returns>
    public ProjectUser? GetUser(string userId)
    {
        return _projectUsers.FirstOrDefault(u => u.UserId == userId);
    }

    /// <summary>
    /// Checks if a user is part of the project.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user is part of the project.</returns>
    public bool HasUser(string userId)
    {
        return _projectUsers.Any(u => u.UserId == userId && u.IsActive);
    }

    /// <summary>
    /// Gets the count of active users.
    /// </summary>
    /// <returns>The number of active users.</returns>
    public int GetActiveUserCount()
    {
        return _projectUsers.Count(u => u.IsActive);
    }

    /// <summary>
    /// Gets the count of users by role.
    /// </summary>
    /// <returns>Dictionary with role as key and count as value.</returns>
    public Dictionary<string, int> GetUserCountByRole()
    {
        return _projectUsers
            .Where(u => u.IsActive)
            .GroupBy(u => u.Role)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Gets users who joined recently.
    /// </summary>
    /// <param name="sinceDate">The date to check from.</param>
    /// <returns>Collection of recently joined users.</returns>
    public IEnumerable<ProjectUser> GetRecentlyJoinedUsers(DateTime sinceDate)
    {
        return _projectUsers.Where(u => u.JoinedAt >= sinceDate && u.IsActive);
    }

    /// <summary>
    /// Adds a user to the project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="role">The role to assign.</param>
    /// <param name="invitedBy">The user ID who invited this user.</param>
    /// <param name="auditContext">The audit context.</param>
    /// <returns>The created project user.</returns>
    public ProjectUser AddUser(string userId, string role, string invitedBy, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        // Check if user already exists
        var existingUser = _projectUsers.FirstOrDefault(u => u.UserId == userId);
        if (existingUser != null)
        {
            if (!existingUser.IsActive)
            {
                // Reactivate user
                existingUser.IsActive = true;
                existingUser.Role = role;
                existingUser.JoinedAt = auditContext.UtcNow;
                existingUser.LeftAt = null;
                existingUser.UpdatedAt = auditContext.UtcNow;
                existingUser.UpdatedBy = auditContext.User;
                return existingUser;
            }

            throw new InvalidOperationException($"User {userId} is already active in the project.");
        }

        var projectUser = new ProjectUser
        {
            ProjectId = this.Id,
            UserId = userId,
            Role = role,
            IsActive = true,
            JoinedAt = auditContext.UtcNow,
            InvitedBy = invitedBy,
            CreatedAt = auditContext.UtcNow
        };

        _projectUsers.Add(projectUser);
        SetUpdated(auditContext);

        return projectUser;
    }

    /// <summary>
    /// Removes a user from the project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="auditContext">The audit context.</param>
    public void RemoveUser(string userId, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var user = _projectUsers.FirstOrDefault(u => u.UserId == userId && u.IsActive);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} is not active in the project.");
        }

        user.IsActive = false;
        user.LeftAt = auditContext.UtcNow;
        user.UpdatedAt = auditContext.UtcNow;
        user.UpdatedBy = auditContext.User;

        SetUpdated(auditContext);
    }
}