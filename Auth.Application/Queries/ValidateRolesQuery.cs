using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Auth.Application.Queries;

public sealed record ValidateRolesQuery(List<string> RoleNames) : IBaseRequest<List<string>>;

public sealed class ValidateRolesQueryHandler(
    IAuthRepository authRepository) : BaseCommandHandler<ValidateRolesQuery, List<string>>
{
    public override async Task<Result<List<string>>> Handle(ValidateRolesQuery request, CancellationToken cancellationToken)
    {
        var validRoles = new List<string>();

        foreach (var roleName in request.RoleNames)
        {
            if (await authRepository.RoleExistsAsync(roleName, cancellationToken))
            {
                validRoles.Add(roleName);
            }
        }

        return Success(validRoles);
    }
}