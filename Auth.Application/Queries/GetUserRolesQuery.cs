using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Auth.Application.Queries;

public record GetUserRolesQuery(string UserId) : IBaseRequest<IReadOnlyList<string>>;

public class GetUserRolesQueryHandler(IAuthRepository authRepository)
    : BaseCommandHandler<GetUserRolesQuery, IReadOnlyList<string>>
{
    public override async Task<Result<IReadOnlyList<string>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var result = await authRepository.GetUserRolesAsync(request.UserId, cancellationToken);

        return Success(result);
    }
}
