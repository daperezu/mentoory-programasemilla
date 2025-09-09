using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Commands.UpdateUserLocation;

public class UpdateUserLocationCommandHandler(
    IUserProfileRepository userProfileRepository,
    IAuditContext auditContext,
    ILogger<UpdateUserLocationCommandHandler> logger)
    : BaseCommandHandler<UpdateUserLocationCommand>
{
    public override async Task<Result> Handle(UpdateUserLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userProfile = await userProfileRepository.GetByUserIdAsync(request.UserId);
            if (userProfile is null)
            {
                return Failure(
                    ResultErrorCodes.Auth_UserNotFound,
                    ("UpdateUserLocation", "Perfil de usuario no encontrado"));
            }

            var result = userProfile.UpdateLocation(
                request.Country,
                request.Province,
                request.Canton,
                request.District,
                request.FullAddress,
                auditContext);
            if (!result.IsSuccess)
            {
                return Failure(
                    result.ErrorCode ?? ResultErrorCodes.GenericError,
                    result.ErrorMessages ?? [("UpdateUserLocation", "Error al actualizar la ubicación")]);
            }

            userProfileRepository.Update(userProfile);
            await userProfileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating location for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.GenericError,
                ("UpdateUserLocation", "Error al actualizar la ubicación del usuario"));
        }
    }
}