using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;

namespace LinaSys.Auth.Application.Queries.Context;

public record GetUserRolesQuery(string UserId) : IBaseRequest<UserRolesDto>;

public class UserRolesDto
{
    public List<string> Roles { get; set; } = [];

    public bool IsGlobalAdministrator { get; set; }
}

public class GetUserRolesQueryHandler(IAuthRepository authRepository) : BaseCommandHandler<GetUserRolesQuery, UserRolesDto>
{
    public override async Task<Result<UserRolesDto>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Failure(ResultErrorCodes.Auth_UserNotFound, (nameof(GetUserRolesQuery), $"User with ID {request.UserId} not found."));
        }

        var userRoles = await authRepository.GetRolesAsync(user, cancellationToken);

        var result = new UserRolesDto
        {
            Roles = userRoles.ToList(),
            IsGlobalAdministrator = userRoles.Contains(Roles.GlobalAdministrator),
        };

        return Success(result);
    }
}
