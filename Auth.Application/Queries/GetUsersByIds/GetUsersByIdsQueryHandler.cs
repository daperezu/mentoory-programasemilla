using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Queries.GetUsersByIds;

/// <summary>
/// Handler for GetUsersByIdsQuery.
/// </summary>
public class GetUsersByIdsQueryHandler(
    IAuthRepository authRepository,
    IMemoryCache cache,
    ILogger<GetUsersByIdsQueryHandler> logger)
    : BaseCommandHandler<GetUsersByIdsQuery, Dictionary<string, UserBasicInfoDto>>
{
    /// <inheritdoc/>
    public override async Task<Result<Dictionary<string, UserBasicInfoDto>>> Handle(
        GetUsersByIdsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.UserIds == null || !request.UserIds.Any())
        {
            return Success(new Dictionary<string, UserBasicInfoDto>());
        }

        var userIds = request.UserIds.Distinct().ToList();
        var result = new Dictionary<string, UserBasicInfoDto>();
        var uncachedIds = new List<string>();

        // Check cache first for each user
        foreach (var userId in userIds)
        {
            var cacheKey = $"user_basic_info_{userId}";
            if (cache.TryGetValue<UserBasicInfoDto>(cacheKey, out var cachedUser))
            {
                result[userId] = cachedUser!;
            }
            else
            {
                uncachedIds.Add(userId);
            }
        }

        // Batch load uncached users
        if (uncachedIds.Any())
        {
            logger.LogDebug("Batch loading {Count} users from database", uncachedIds.Count);

            var users = await authRepository.GetUsersByIdsAsync(uncachedIds, cancellationToken);

            foreach (var user in users)
            {
                var userDto = new UserBasicInfoDto
                {
                    Id = user.Id,
                    FirstName = user.UserName ?? "Usuario",  // Use UserName as placeholder
                    LastName = string.Empty,
                    Email = user.Email ?? string.Empty,
                };

                result[user.Id] = userDto;

                // Cache individual user for 10 minutes
                var cacheKey = $"user_basic_info_{user.Id}";
                cache.Set(cacheKey, userDto, TimeSpan.FromMinutes(10));
            }

            // Add empty entries for users not found
            foreach (var userId in uncachedIds.Where(id => !result.ContainsKey(id)))
            {
                var emptyUser = new UserBasicInfoDto
                {
                    Id = userId,
                    FirstName = "Usuario",
                    LastName = "Desconocido",
                    Email = string.Empty,
                };
                result[userId] = emptyUser;
            }
        }

        logger.LogInformation(
            "Loaded {TotalCount} users ({CachedCount} from cache, {LoadedCount} from database)",
            result.Count,
            userIds.Count - uncachedIds.Count,
            uncachedIds.Count);

        return Success(result);
    }
}