using LinaSys.Auth.Application.Interfaces;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Infrastructure.Services;

/// <summary>
/// Service for handling project access operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProjectAccessService"/> class.
/// </remarks>
/// <param name="repository">The auth repository.</param>
/// <param name="cache">The memory cache.</param>
/// <param name="logger">The logger.</param>
public class ProjectAccessService(
    IAuthRepository repository,
    IMemoryCache cache,
    ILogger<ProjectAccessService> logger) : IProjectAccessService
{

    /// <inheritdoc/>
    public async Task<List<ProjectInfo>> GetUserProjectsAsync(string userId, string role, long incubatorId, CancellationToken cancellationToken = default)
    {
        // Special handling for Global Administrator - they can access all projects
        if (role == Roles.GlobalAdministrator)
        {
            logger.LogDebug("User {UserId} is Global Administrator, returning marker for all projects in incubator {IncubatorId}", userId, incubatorId);

            // Return a special marker that indicates all projects should be fetched
            // The ProjectId = -1 signals to the orchestration layer to fetch all projects
            return new List<ProjectInfo>
            {
                new ProjectInfo
                {
                    ProjectId = -1, // Special marker for "all projects"
                    UserRole = Roles.GlobalAdministrator
                }
            };
        }

        // Try to get from cache first
        var cacheKey = $"user-projects:{userId}:{incubatorId}";
        if (cache.TryGetValue<List<ProjectInfo>>(cacheKey, out var cachedProjects) && cachedProjects != null)
        {
            logger.LogDebug("Returning cached projects for user {UserId} in incubator {IncubatorId}", userId, incubatorId);
            return cachedProjects;
        }

        // Get all active project access records for the user
        var projectAccesses = await repository.GetUserProjectAccessesAsync(userId, cancellationToken);

        // Filter by incubator if needed (incubatorId from access entity)
        // Note: We store incubatorId in UserProjectAccess for filtering
        var filteredAccesses = projectAccesses
            .Where(pa => pa.IsActive && (incubatorId == 0 || pa.IncubatorId == incubatorId))
            .ToList();

        // Convert to ProjectInfo DTOs
        var projects = filteredAccesses.Select(pa => new ProjectInfo
        {
            ProjectId = pa.ProjectId,
            UserRole = pa.Role // Include user's role in the project
        }).ToList();

        // Cache the results for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, projects, cacheOptions);

        logger.LogInformation("Retrieved {Count} projects for user {UserId} in incubator {IncubatorId}",
            projects.Count, userId, incubatorId);

        return projects;
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateProjectAccessAsync(string userId, long projectId, long incubatorId, CancellationToken cancellationToken = default)
    {
        // Get the specific project access record
        var projectAccess = await repository.GetUserProjectAccessAsync(userId, projectId, cancellationToken);

        // Check if access exists and is active
        var hasAccess = projectAccess is not null && projectAccess.IsActive;

        // If has access and incubatorId is specified, verify it matches
        if (hasAccess && incubatorId > 0)
        {
            hasAccess = projectAccess!.IncubatorId == incubatorId;
        }

        logger.LogDebug("User {UserId} access to project {ProjectId}: {HasAccess}",
            userId, projectId, hasAccess);

        return hasAccess;
    }

    /// <inheritdoc/>
    public async Task<bool> ProjectExistsInIncubatorAsync(long projectId, long incubatorId, CancellationToken cancellationToken = default)
    {
        // Check if any user has access to this project in the specified incubator
        var projectAccesses = await repository.GetProjectAccessesByProjectIdAsync(projectId, cancellationToken);

        // Check if any access record exists for this project in the specified incubator
        var exists = projectAccesses.Any(pa => pa.IsActive && pa.IncubatorId == incubatorId);

        logger.LogDebug("Project {ProjectId} exists in incubator {IncubatorId}: {Exists}",
            projectId, incubatorId, exists);

        return exists;
    }

    /// <summary>
    /// Invalidates cached data for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public void InvalidateUserCache(string userId)
    {
        // Remove all cache entries for this user
        // This would be called when user's access changes
        logger.LogDebug("Invalidating cache for user {UserId}", userId);
        // Note: In production, consider using a more sophisticated cache key pattern
        // or a distributed cache with tag-based invalidation
    }
}
