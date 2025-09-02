using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Commands.DeactivateUser;

public class DeactivateUserCommandHandler(
    IUserProfileRepository userProfileRepository,
    ILogger<DeactivateUserCommandHandler> logger)
    : BaseCommandHandler<DeactivateUserCommand>
{
    public override async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var userProfile = await userProfileRepository.GetByUserIdAsync(request.UserId);

        if (userProfile is null)
        {
            return Failure(ResultErrorCodes.Auth_UserNotFound, (nameof(request.UserId), $"Usuario con ID {request.UserId} no encontrado"));
        }

        if (!userProfile.IsActive)
        {
            return Failure(ResultErrorCodes.AlreadyInactive, ("UserProfile", "El usuario ya está inactivo"));
        }

        userProfile.Deactivate();

        userProfileRepository.Update(userProfile);
        await userProfileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        logger.LogInformation("Usuario {UserId} desactivado exitosamente", request.UserId);

        return Success();
    }
}

