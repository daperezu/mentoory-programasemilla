using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Permissions.Application.ProtectedResource.Queries;

public record UserHasAccessToProtectedResourceQuery(string UserId, long InternalProtectedResourceId) : IBaseRequest;

public class UserHasAccessToProtectedResourceQueryHandler(IProtectedResourceRepository protectedResourceRepository)
    : BaseCommandHandler<UserHasAccessToProtectedResourceQuery>
{
    public override async Task<Result> Handle(UserHasAccessToProtectedResourceQuery request, CancellationToken cancellationToken)
    {
        var result = await protectedResourceRepository.UserHasAccessAsync(request.UserId, request.InternalProtectedResourceId, cancellationToken);
        return result
            ? Success()
            : Failure(ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource, (nameof(UserHasAccessToProtectedResourceQuery), "User does not have access to the protected resource."));
    }
}
