using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Commands.ReactivateUser;

public class ReactivateUserCommandHandler(
    IUserProfileRepository userProfileRepository,
    ILogger<ReactivateUserCommandHandler> logger)
    : BaseCommandHandler<ReactivateUserCommand>
{
    public override async Task<Result> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var userProfile = await userProfileRepository.GetByUserIdAsync(request.UserId);

        if (userProfile is null)
        {
            return Failure(ResultErrorCodes.Auth_UserNotFound, (nameof(request.UserId), $"Usuario con ID {request.UserId} no encontrado"));
        }

        if (userProfile.IsActive)
        {
            return Failure(ResultErrorCodes.AlreadyActive, ("UserProfile", "El usuario ya está activo"));
        }

        userProfile.Reactivate();

        userProfileRepository.Update(userProfile);
        await userProfileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        logger.LogInformation("Usuario {UserId} reactivado exitosamente", request.UserId);

        return Success();
    }
}

