using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Queries;

public record GetUserEmailQuery(string UserId) : IBaseRequest<string?>;

public class GetUserEmailQueryHandler(
    IAuthRepository authRepository,
    ILogger<GetUserEmailQueryHandler> logger)
    : BaseCommandHandler<GetUserEmailQuery, string?>
{
    public override async Task<Result<string?>> Handle(GetUserEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("User {UserId} not found when fetching email", request.UserId);
                return Success(null);
            }

            return Success(user.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching email for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.Auth_QueryFailed,
                (nameof(request.UserId), "Error al obtener el email del usuario."));
        }
    }
}
