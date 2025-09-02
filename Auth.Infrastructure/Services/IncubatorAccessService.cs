using LinaSys.Auth.Application.Interfaces;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Infrastructure.Services;

/// <summary>
/// Service for handling incubator access operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IncubatorAccessService"/> class.
/// </remarks>
/// <param name="repository">The auth repository.</param>
/// <param name="roleManager">The role manager.</param>
/// <param name="logger">The logger.</param>
public class IncubatorAccessService(
    IAuthRepository repository,
    RoleManager<IdentityRole> roleManager,
    ILogger<IncubatorAccessService> logger) : IIncubatorAccessService
{

    /// <inheritdoc/>
    public async Task<List<long>> GetUserActiveIncubatorsAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        // Get all incubator access records for the user
        var incubatorAccesses = await repository.GetUserIncubatorAccessesAsync(userId, cancellationToken);

        // Filter active accesses and get distinct incubator IDs
        var activeIncubatorIds = incubatorAccesses
            .Where(ia => ia.IsActive)
            .Select(ia => ia.IncubatorId)
            .Distinct()
            .ToList();

        logger.LogInformation("Retrieved {Count} incubators for user {UserId}", activeIncubatorIds.Count, userId);

        return activeIncubatorIds;
    }

    /// <inheritdoc/>
    public async Task<bool> IncubatorExistsAsync(long incubatorId, CancellationToken cancellationToken = default)
    {
        // Check if any user has access to this incubator
        var incubatorAccesses = await repository.GetIncubatorAccessesByIncubatorIdAsync(incubatorId, cancellationToken);

        // Check if any access record exists for this incubator
        var exists = incubatorAccesses.Any(ia => ia.IsActive);

        logger.LogDebug("Incubator {IncubatorId} exists: {Exists}", incubatorId, exists);

        return exists;
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateIncubatorAccessAsync(string userId, string role, long incubatorId, CancellationToken cancellationToken = default)
    {
        // Get the specific incubator access record
        var incubatorAccess = await repository.GetUserIncubatorAccessAsync(userId, incubatorId, cancellationToken);

        // Check if access exists and is active
        var hasAccess = incubatorAccess is not null && incubatorAccess.IsActive;

        if (!hasAccess && !string.IsNullOrEmpty(role))
        {
            // Check if user has a global role that grants access
            var roleEntity = await roleManager.FindByNameAsync(role);

            switch (roleEntity?.Name)
            {
                case Roles.GlobalAdministrator:
                    hasAccess = true; // Administrators have access to all incubators
                    logger.LogDebug("User {UserId} has GlobalAdministrator access to incubator {IncubatorId}", userId, incubatorId);
                    break;
            }
        }

        logger.LogDebug("User {UserId} access to incubator {IncubatorId}: {HasAccess}", userId, incubatorId, hasAccess);

        return hasAccess;
    }
}
