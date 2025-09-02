using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Queries.GetUserPreferences;

public record GetUserPreferencesQuery(string UserId) : IBaseRequest<Dictionary<string, string>>;

public class GetUserPreferencesQueryHandler(
    IUserProfileRepository userProfileRepository,
    ILogger<GetUserPreferencesQueryHandler> logger)
    : BaseCommandHandler<GetUserPreferencesQuery, Dictionary<string, string>>
{
    public override async Task<Result<Dictionary<string, string>>> Handle(
        GetUserPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userProfile = await userProfileRepository.GetByUserIdAsync(request.UserId);
            if (userProfile is null)
            {
                logger.LogWarning("User profile not found for user ID {UserId}", request.UserId);
                // Return empty preferences for users without profile
                return Success(new Dictionary<string, string>());
            }

            var preferences = userProfile.Preferences
                .ToDictionary(p => p.Key, p => p.Value);

            logger.LogDebug(
                "Retrieved {Count} preferences for user {UserId}",
                preferences.Count,
                request.UserId);

            return Success(preferences);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving preferences for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al obtener las preferencias."));
        }
    }
}