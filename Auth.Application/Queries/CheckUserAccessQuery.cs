using LinaSys.Auth.Application.Interfaces;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;

namespace LinaSys.Auth.Application.Queries;

public sealed record CheckUserAccessQuery(
    string UserId,
    string AccessType,
    long? ResourceId = null) : IBaseRequest<bool>;

public sealed class CheckUserAccessQueryHandler(
    IAuthRepository authRepository,
    IProjectAccessService projectAccessService,
    IIncubatorAccessService incubatorAccessService) : BaseCommandHandler<CheckUserAccessQuery, bool>
{
    public override async Task<Result<bool>> Handle(CheckUserAccessQuery request, CancellationToken cancellationToken)
    {
        // Check if user exists
        var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Success(false);
        }

        // Check for global administrator role
        var userRoles = await authRepository.GetUserRolesAsync(request.UserId, cancellationToken);
        if (userRoles.Contains(Roles.GlobalAdministrator))
        {
            return Success(true);
        }

        // Check access based on type
        var hasAccess = request.AccessType.ToLower() switch
        {
            "project" when request.ResourceId.HasValue =>
                await projectAccessService.ValidateProjectAccessAsync(request.UserId, request.ResourceId.Value, 0, cancellationToken),
            "incubator" when request.ResourceId.HasValue =>
                await incubatorAccessService.ValidateIncubatorAccessAsync(
                    request.UserId,
                    userRoles.FirstOrDefault() ?? string.Empty,
                    request.ResourceId.Value,
                    cancellationToken),
            _ => false
        };

        return Success(hasAccess);
    }
}