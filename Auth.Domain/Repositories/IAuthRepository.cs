using LinaSys.Auth.Domain.AggregatesModel.Access;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Auth.Domain.Repositories;

/// <summary>
/// Interface for the authentication repository.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Gets the unit of work for the repository.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }

    #region User Management Operations

    /// <summary>
    /// Finds a user by their identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<AggregatesModel.User.User?> FindUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user by their username (identification number).
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<AggregatesModel.User.User?> FindUserByNameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user by their email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<AggregatesModel.User.User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by a list of email addresses.
    /// </summary>
    /// <param name="emails">The list of email addresses.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Dictionary of email to user.</returns>
    Task<Dictionary<string, AggregatesModel.User.User>> GetUsersByEmailsAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by a list of user IDs for batch loading to avoid N+1 queries.
    /// </summary>
    /// <param name="userIds">The list of user IDs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of users found.</returns>
    Task<List<AggregatesModel.User.User>> GetUsersByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user with the specified password.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(AggregatesModel.User.User user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a new username for the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="newUsername">The new username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> SetUserNameAsync(AggregatesModel.User.User user, string newUsername, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the user's email address.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="newEmail">The new email.</param>
    /// <param name="token">The change email token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> ChangeEmailAsync(AggregatesModel.User.User user, string newEmail, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the user's email is confirmed.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if email is confirmed, false otherwise.</returns>
    Task<bool> IsEmailConfirmedAsync(AggregatesModel.User.User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms the user's email address using a confirmation token.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="token">The confirmation token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> ConfirmEmailAsync(AggregatesModel.User.User user, string token, CancellationToken cancellationToken = default);

    #endregion

    #region Role Management Operations

    /// <summary>
    /// Gets the roles of a user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of role names.</returns>
    Task<IReadOnlyList<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the roles of a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of role names.</returns>
    Task<IList<string>> GetRolesAsync(AggregatesModel.User.User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user to a role.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="role">The role name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> AddToRoleAsync(AggregatesModel.User.User user, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user to multiple roles.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The role names.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> AddToRolesAsync(AggregatesModel.User.User user, IEnumerable<string> roles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user from multiple roles.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The role names.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> RemoveFromRolesAsync(AggregatesModel.User.User user, IEnumerable<string> roles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users in a specific role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of users in the role.</returns>
    Task<IList<AggregatesModel.User.User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role exists in the system.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the role exists, false otherwise.</returns>
    Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the user's password using a reset token.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="token">The reset token.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> ResetPasswordAsync(AggregatesModel.User.User user, string token, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(AggregatesModel.User.User user, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the user has a password set.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if user has a password, false otherwise.</returns>
    Task<bool> HasPasswordAsync(AggregatesModel.User.User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a password to a user that doesn't have one.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="password">The password to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> AddPasswordAsync(AggregatesModel.User.User user, string password, CancellationToken cancellationToken = default);

    #endregion

    #region Token Generation Operations

    /// <summary>
    /// Generates a change email token for the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="newEmail">The new email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated token.</returns>
    Task<string> GenerateChangeEmailTokenAsync(AggregatesModel.User.User user, string newEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a password reset token for the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated token.</returns>
    Task<string> GeneratePasswordResetTokenAsync(AggregatesModel.User.User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an email confirmation token for the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated token.</returns>
    Task<string> GenerateEmailConfirmationTokenAsync(AggregatesModel.User.User user, CancellationToken cancellationToken = default);

    #endregion

    #region User Context Operations

    /// <summary>
    /// Gets the user ID from a ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The user ID.</returns>
    string? GetUserId(System.Security.Claims.ClaimsPrincipal principal);

    /// <summary>
    /// Gets the user ID from a User object.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The user ID.</returns>
    Task<string> GetUserIdAsync(AggregatesModel.User.User user);

    /// <summary>
    /// Gets a user from a ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<AggregatesModel.User.User?> GetUserAsync(System.Security.Claims.ClaimsPrincipal principal, CancellationToken cancellationToken = default);

    #endregion

    #region Profile Operations

    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The phone number if set, null otherwise.</returns>
    Task<string?> GetPhoneNumberAsync(AggregatesModel.User.User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the user's phone number.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="phoneNumber">The phone number to set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Identity result indicating success or failure.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> SetPhoneNumberAsync(AggregatesModel.User.User user, string? phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's username.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The username.</returns>
    Task<string?> GetUserNameAsync(AggregatesModel.User.User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The email address.</returns>
    Task<string?> GetEmailAsync(AggregatesModel.User.User user, CancellationToken cancellationToken = default);

    #endregion

    #region Access Control Operations

    /// <summary>
    /// Gets a user's project access record.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user project access record if found, null otherwise.</returns>
    Task<UserProjectAccess?> GetUserProjectAccessAsync(string userId, long projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new user project access record.
    /// </summary>
    /// <param name="userProjectAccess">The user project access to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task AddUserProjectAccessAsync(UserProjectAccess userProjectAccess, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user project access record.
    /// </summary>
    /// <param name="userProjectAccess">The user project access to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateUserProjectAccessAsync(UserProjectAccess userProjectAccess, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a user's incubator access record.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="incubatorId">The incubator identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user incubator access record if found, null otherwise.</returns>
    Task<UserIncubatorAccess?> GetUserIncubatorAccessAsync(string userId, long incubatorId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new user incubator access record.
    /// </summary>
    /// <param name="userIncubatorAccess">The user incubator access to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task AddUserIncubatorAccessAsync(UserIncubatorAccess userIncubatorAccess, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user incubator access record.
    /// </summary>
    /// <param name="userIncubatorAccess">The user incubator access to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateUserIncubatorAccessAsync(UserIncubatorAccess userIncubatorAccess, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an active mentorship access record for a starter in a project.
    /// </summary>
    /// <param name="starterUserId">The starter user identifier.</param>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user mentorship access record if found, null otherwise.</returns>
    Task<UserMentorshipAccess?> GetActiveMentorshipAccessByStarterAsync(string starterUserId, long projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a mentorship access record.
    /// </summary>
    /// <param name="mentorUserId">The mentor user identifier.</param>
    /// <param name="starterUserId">The starter user identifier.</param>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user mentorship access record if found, null otherwise.</returns>
    Task<UserMentorshipAccess?> GetMentorshipAccessAsync(string mentorUserId, string starterUserId, long projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new user mentorship access record.
    /// </summary>
    /// <param name="userMentorshipAccess">The user mentorship access to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task AddUserMentorshipAccessAsync(UserMentorshipAccess userMentorshipAccess, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user mentorship access record.
    /// </summary>
    /// <param name="userMentorshipAccess">The user mentorship access to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateUserMentorshipAccessAsync(UserMentorshipAccess userMentorshipAccess, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all project access records for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of user project access records.</returns>
    Task<List<UserProjectAccess>> GetUserProjectAccessesAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all incubator access records for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of user incubator access records.</returns>
    Task<List<UserIncubatorAccess>> GetUserIncubatorAccessesAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all access records for a specific project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of user project access records.</returns>
    Task<List<UserProjectAccess>> GetProjectAccessesByProjectIdAsync(long projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all access records for a specific incubator.
    /// </summary>
    /// <param name="incubatorId">The incubator identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of user incubator access records.</returns>
    Task<List<UserIncubatorAccess>> GetIncubatorAccessesByIncubatorIdAsync(long incubatorId, CancellationToken cancellationToken);

    #endregion
}
