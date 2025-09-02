using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Auth.Application.Queries;

public sealed record CheckUserAccessQuery(
    string UserId,
    string AccessType,
    long? ResourceId = null) : IBaseRequest<bool>;

public sealed class CheckUserAccessQueryHandler(
    IAuthRepository authRepository) : BaseCommandHandler<CheckUserAccessQuery, bool>
{
    public override async Task<Result<bool>> Handle(CheckUserAccessQuery request, CancellationToken cancellationToken)
    {
        // Check if user exists
        var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Success(false);
        }

        // Check access based on type
        var hasAccess = request.AccessType.ToLower() switch
        {
            "project" when request.ResourceId.HasValue =>
                await authRepository.GetUserProjectAccessAsync(request.UserId, request.ResourceId.Value, cancellationToken) is not null,
            "incubator" when request.ResourceId.HasValue =>
                await authRepository.GetUserIncubatorAccessAsync(request.UserId, request.ResourceId.Value, cancellationToken) is not null,
            _ => false
        };

        return Success(hasAccess);
    }
}