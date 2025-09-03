using LinaSys.Auth.Application.Interfaces;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Auth.Domain.ValueObjects;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;

namespace LinaSys.Auth.Application.Queries.Context;

/// <summary>
/// Query to validate if a user context is still valid.
/// </summary>
public record ValidateUserContextQuery(UserContext Context) : IBaseRequest<bool>;

/// <summary>
/// Handler for ValidateUserContextQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidateUserContextQueryHandler"/> class.
/// </remarks>
/// <param name="authRepository">The auth repository.</param>
/// <param name="userContextRepository">The user context repository.</param>
/// <param name="incubatorAccessService">The incubator access service.</param>
/// <param name="projectAccessService">The project access service.</param>
public class ValidateUserContextQueryHandler(
    IAuthRepository authRepository,
    IUserContextRepository userContextRepository,
    IIncubatorAccessService incubatorAccessService,
    IProjectAccessService projectAccessService) : BaseCommandHandler<ValidateUserContextQuery, bool>
{

    /// <inheritdoc/>
    public override async Task<Result<bool>> Handle(
        ValidateUserContextQuery request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        // Check if context has at least a role
        if (string.IsNullOrEmpty(context.Role))
        {
            return Success(false);
        }

        // Validate user exists
        var userExists = await ValidateUserExists(context.UserId, cancellationToken);
        if (!userExists)
        {
            return Success(false);
        }

        // Validate role assignment
        var hasRole = await ValidateUserHasRole(context.UserId, context.Role, cancellationToken);
        if (!hasRole)
        {
            return Success(false);
        }

        // Global Administrator can access any incubator/project
        if (context.IsGlobalAdministrator)
        {
            // Just validate that incubator and project exist if specified
            if (context.IncubatorId.HasValue)
            {
                var incubatorExists = await ValidateIncubatorExists(context.IncubatorId.Value, cancellationToken);
                if (!incubatorExists)
                {
                    return Success(false);
                }

                if (context.ProjectId.HasValue)
                {
                    var projectExists = await ValidateProjectExistsInIncubator(
                        context.ProjectId.Value,
                        context.IncubatorId.Value,
                        cancellationToken);
                    if (!projectExists)
                    {
                        return Success(false);
                    }
                }
            }

            return Success(true);
        }

        // Check if user is Administrator (needs only incubator)
        var isAdmin = context.Role == Roles.Administrator;

        // For Administrator role, only incubator is required
        if (isAdmin)
        {
            if (!context.IncubatorId.HasValue)
            {
                return Success(false);
            }

            // Validate incubator access
            var hasIncubatorAccess = await ValidateIncubatorAccess(
                context.UserId,
                context.Role,
                context.IncubatorId.Value,
                cancellationToken);

            return Success(hasIncubatorAccess);
        }

        // For other roles, validate both incubator and project access
        if (!context.IncubatorId.HasValue || !context.ProjectId.HasValue)
        {
            return Success(false);
        }

        // Validate incubator access through ProtectedResources
        var hasIncubatorAccessForOtherRoles = await ValidateIncubatorAccess(
            context.UserId,
            context.Role,
            context.IncubatorId.Value,
            cancellationToken);
        if (!hasIncubatorAccessForOtherRoles)
        {
            return Success(false);
        }

        // Validate project access through ProjectUsers
        var hasProjectAccess = await ValidateProjectAccess(
            context.UserId,
            context.ProjectId.Value,
            context.IncubatorId.Value,
            cancellationToken);
        if (!hasProjectAccess)
        {
            return Success(false);
        }

        return Success(true);
    }

    private async Task<bool> ValidateUserExists(string userId, CancellationToken cancellationToken)
    {
        var user = await authRepository.FindUserByIdAsync(userId, cancellationToken);
        return user != null;
    }

    private Task<bool> ValidateUserHasRole(string userId, string role, CancellationToken cancellationToken)
    {
        return userContextRepository.UserHasRoleAsync(userId, role, cancellationToken);
    }

    private async Task<bool> ValidateIncubatorExists(long incubatorId, CancellationToken cancellationToken)
    {
        return await incubatorAccessService.IncubatorExistsAsync(incubatorId, cancellationToken);
    }

    private async Task<bool> ValidateProjectExistsInIncubator(long projectId, long incubatorId, CancellationToken cancellationToken)
    {
        return await projectAccessService.ProjectExistsInIncubatorAsync(projectId, incubatorId, cancellationToken);
    }

    private async Task<bool> ValidateIncubatorAccess(string userId, string role, long incubatorId, CancellationToken cancellationToken)
    {
        return await incubatorAccessService.ValidateIncubatorAccessAsync(userId, role, incubatorId, cancellationToken);
    }

    private async Task<bool> ValidateProjectAccess(string userId, long projectId, long incubatorId, CancellationToken cancellationToken)
    {
        return await projectAccessService.ValidateProjectAccessAsync(userId, projectId, incubatorId, cancellationToken);
    }
}
