using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;

namespace LinaSys.Auth.Application.Queries.Context;

/// <summary>
/// Query to get the last user context with enriched role information.
/// </summary>
public record GetEnrichedLastUserContextQuery(string UserId) : IBaseRequest<AuthEnrichedUserContextDto?>;

/// <summary>
/// DTO containing user context with enriched role information from Auth domain.
/// </summary>
public record AuthEnrichedUserContextDto(
    string UserId,
    string? Role,
    long? IncubatorId,
    long? ProjectId,
    bool IsGlobalAdministrator);

/// <summary>
/// Handler for GetEnrichedLastUserContextQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetEnrichedLastUserContextQueryHandler"/> class.
/// </remarks>
/// <param name="userContextRepository">The user context repository.</param>
/// <param name="roleManager">The role manager.</param>
public class GetEnrichedLastUserContextQueryHandler(IUserContextRepository userContextRepository)
    : BaseCommandHandler<GetEnrichedLastUserContextQuery, AuthEnrichedUserContextDto?>
{

    /// <inheritdoc/>
    public override async Task<Result<AuthEnrichedUserContextDto?>> Handle(
        GetEnrichedLastUserContextQuery request,
        CancellationToken cancellationToken)
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

        // Create enriched context
        AuthEnrichedUserContextDto? context;
        if (isGlobalAdmin && preferences.LastRole != null)
        {
            context = new AuthEnrichedUserContextDto(
                request.UserId,
                preferences.LastRole,
                preferences.LastIncubatorId,
                preferences.LastProjectId,
                true);
        }
        else if (preferences is { LastRole: not null, LastIncubatorId: not null, LastProjectId: not null })
        {
            context = new AuthEnrichedUserContextDto(
                request.UserId,
                preferences.LastRole,
                preferences.LastIncubatorId.Value,
                preferences.LastProjectId.Value,
                false);
        }
        else
        {
            // Incomplete preferences
            return Success(null);
        }

        return Success(context);
    }
}
