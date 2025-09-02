using LinaSys.Auth.Domain.Repositories;
using LinaSys.Auth.Domain.ValueObjects;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;

namespace LinaSys.Auth.Application.Commands.Context;

public record GetLastUserContextCommand(string UserId) : IBaseRequest<UserContext?>;

public class GetLastUserContextCommandHandler(IUserContextRepository userContextRepository)
    : BaseCommandHandler<GetLastUserContextCommand, UserContext?>
{

    /// <inheritdoc/>
    public override async Task<Result<UserContext?>> Handle(GetLastUserContextCommand request, CancellationToken cancellationToken)
    {
        // Load saved preferences
        var preferences = await userContextRepository.GetUserContextPreferencesAsync(
            request.UserId,
            cancellationToken);

        if (preferences == null)
        {
            return Success(null);
        }

        // Check if user is global admin
        var isGlobalAdmin = await userContextRepository.UserHasRoleAsync(
            request.UserId,
            Roles.GlobalAdministrator,
            cancellationToken);

        // Create context from saved preferences
        UserContext context;
        if (isGlobalAdmin && preferences.LastRole != null)
        {
            context = UserContext.CreateForGlobalAdministrator(
                request.UserId,
                preferences.LastRole,
                preferences.LastIncubatorId,
                preferences.LastProjectId);
        }
        else if (preferences is { LastRole: not null, LastIncubatorId: not null, LastProjectId: not null })
        {
            context = UserContext.CreateForUser(
                request.UserId,
                preferences.LastRole,
                preferences.LastIncubatorId.Value,
                preferences.LastProjectId.Value);
        }
        else
        {
            // Incomplete preferences
            return Success(null);
        }

        return Success(context);
    }
}
