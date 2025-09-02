using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Permissions.Application.ProtectedResource.Queries;

public record RoleHasAccessToProtectedResourceQuery(List<string> Roles, long ProtectedResourceId) : IBaseRequest;

public class RoleHasAccessToProtectedResourceQueryHandler(IProtectedResourceRepository protectedResourceRepository)
    : BaseCommandHandler<RoleHasAccessToProtectedResourceQuery>
{
    public override async Task<Result> Handle(RoleHasAccessToProtectedResourceQuery request, CancellationToken cancellationToken)
    {
        var result = await protectedResourceRepository.RoleHasAccessAsync(request.Roles, request.ProtectedResourceId, cancellationToken);
        return result ? Success() : Failure(ResultErrorCodes.Auth_RolesHasNoAccessToProtectedResource);
    }
}
