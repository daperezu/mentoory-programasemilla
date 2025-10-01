using LinaSys.Auth.Domain.AggregatesModel.Access;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Auth.Infrastructure.Persistence;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Auth.Infrastructure.Repositories;

public class AuthRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AuthDbContext context) : IAuthRepository
{
    public IUnitOfWork UnitOfWork => context;

    #region User Management Operations

    public async Task<User?> FindUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await userManager.FindByIdAsync(userId);
    }

    public async Task<User?> FindUserByNameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await userManager.FindByNameAsync(username);
    }

    public async Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await userManager.FindByEmailAsync(email);
    }

    public async Task<Dictionary<string, User>> GetUsersByEmailsAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default)
    {
        var emailList = emails.ToList();
        var users = await userManager.Users
            .Where(u => u.Email != null && emailList.Contains(u.Email))
            .ToListAsync(cancellationToken);

        return users.ToDictionary(u => u.Email!, u => u);
    }

    public async Task<List<User>> GetUsersByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        var idList = userIds.ToList();
        if (!idList.Any())
        {
            return new List<User>();
        }

        return await userManager.Users
            .Where(u => idList.Contains(u.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await userManager.CreateAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> SetUserNameAsync(User user, string newUsername, CancellationToken cancellationToken = default)
    {
        var result = await userManager.SetUserNameAsync(user, newUsername);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> ChangeEmailAsync(User user, string newEmail, string token, CancellationToken cancellationToken = default)
    {
        var result = await userManager.ChangeEmailAsync(user, newEmail, token);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        var result = await userManager.ConfirmEmailAsync(user, token);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    #endregion

    #region Role Management Operations

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is null ? [] : (IReadOnlyList<string>)await userManager.GetRolesAsync(user);
    }

    public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userManager.GetRolesAsync(user);
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> AddToRoleAsync(User user, string role, CancellationToken cancellationToken = default)
    {
        var result = await userManager.AddToRoleAsync(user, role);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> AddToRolesAsync(User user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var result = await userManager.AddToRolesAsync(user, roles);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> RemoveFromRolesAsync(User user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var result = await userManager.RemoveFromRolesAsync(user, roles);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await userManager.GetUsersInRoleAsync(roleName);
    }

    public async Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await roleManager.RoleExistsAsync(roleName);
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(User user, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userManager.HasPasswordAsync(user);
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> AddPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await userManager.AddPasswordAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    #endregion

    #region Token Generation Operations

    public async Task<string> GenerateChangeEmailTokenAsync(User user, string newEmail, CancellationToken cancellationToken = default)
    {
        return await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    #endregion

    #region User Context Operations

    public string? GetUserId(System.Security.Claims.ClaimsPrincipal principal)
    {
        return userManager.GetUserId(principal);
    }

    public async Task<string> GetUserIdAsync(User user)
    {
        return await userManager.GetUserIdAsync(user);
    }

    public async Task<User?> GetUserAsync(System.Security.Claims.ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        return await userManager.GetUserAsync(principal);
    }

    #endregion

    #region Profile Operations

    public async Task<string?> GetPhoneNumberAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userManager.GetPhoneNumberAsync(user);
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> SetPhoneNumberAsync(User user, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        var result = await userManager.SetPhoneNumberAsync(user, phoneNumber);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userManager.GetUserNameAsync(user);
    }

    public async Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userManager.GetEmailAsync(user);
    }

    #endregion

    #region Access Control Operations

    public async Task<UserProjectAccess?> GetUserProjectAccessAsync(string userId, long projectId, CancellationToken cancellationToken)
    {
        return await context.UserProjectAccesses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProjectId == projectId, cancellationToken);
    }

    public Task AddUserProjectAccessAsync(UserProjectAccess userProjectAccess, CancellationToken cancellationToken)
    {
        context.UserProjectAccesses.Add(userProjectAccess);
        return Task.CompletedTask;
    }

    public Task UpdateUserProjectAccessAsync(UserProjectAccess userProjectAccess, CancellationToken cancellationToken)
    {
        context.UserProjectAccesses.Update(userProjectAccess);
        return Task.CompletedTask;
    }

    public async Task<UserIncubatorAccess?> GetUserIncubatorAccessAsync(string userId, long incubatorId, CancellationToken cancellationToken)
    {
        return await context.UserIncubatorAccesses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IncubatorId == incubatorId, cancellationToken);
    }

    public Task AddUserIncubatorAccessAsync(UserIncubatorAccess userIncubatorAccess, CancellationToken cancellationToken)
    {
        context.UserIncubatorAccesses.Add(userIncubatorAccess);
        return Task.CompletedTask;
    }

    public Task UpdateUserIncubatorAccessAsync(UserIncubatorAccess userIncubatorAccess, CancellationToken cancellationToken)
    {
        context.UserIncubatorAccesses.Update(userIncubatorAccess);
        return Task.CompletedTask;
    }

    public async Task<UserMentorshipAccess?> GetActiveMentorshipAccessByStarterAsync(string starterUserId, long projectId, CancellationToken cancellationToken)
    {
        return await context.UserMentorshipAccesses
            .FirstOrDefaultAsync(x => x.StarterUserId == starterUserId && x.ProjectId == projectId && x.IsActive, cancellationToken);
    }

    public async Task<UserMentorshipAccess?> GetMentorshipAccessAsync(string mentorUserId, string starterUserId, long projectId, CancellationToken cancellationToken)
    {
        return await context.UserMentorshipAccesses
            .FirstOrDefaultAsync(x => x.MentorUserId == mentorUserId && x.StarterUserId == starterUserId && x.ProjectId == projectId && x.IsActive, cancellationToken);
    }

    public Task AddUserMentorshipAccessAsync(UserMentorshipAccess userMentorshipAccess, CancellationToken cancellationToken)
    {
        context.UserMentorshipAccesses.Add(userMentorshipAccess);
        return Task.CompletedTask;
    }

    public Task UpdateUserMentorshipAccessAsync(UserMentorshipAccess userMentorshipAccess, CancellationToken cancellationToken)
    {
        context.UserMentorshipAccesses.Update(userMentorshipAccess);
        return Task.CompletedTask;
    }

    public async Task<List<UserProjectAccess>> GetUserProjectAccessesAsync(string userId, CancellationToken cancellationToken)
    {
        return await context.UserProjectAccesses
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserIncubatorAccess>> GetUserIncubatorAccessesAsync(string userId, CancellationToken cancellationToken)
    {
        return await context.UserIncubatorAccesses
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserProjectAccess>> GetProjectAccessesByProjectIdAsync(long projectId, CancellationToken cancellationToken)
    {
        return await context.UserProjectAccesses
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserIncubatorAccess>> GetIncubatorAccessesByIncubatorIdAsync(long incubatorId, CancellationToken cancellationToken)
    {
        return await context.UserIncubatorAccesses
            .Where(x => x.IncubatorId == incubatorId)
            .ToListAsync(cancellationToken);
    }

    #endregion
}
