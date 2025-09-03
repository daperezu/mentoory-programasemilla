using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler(
    IUserProfileRepository userProfileRepository,
    IAuditContext auditContext,
    ILogger<UpdateUserProfileCommandHandler> logger)
    : BaseCommandHandler<UpdateUserProfileCommand>
{
    public override async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userProfile = await userProfileRepository.GetByUserIdAsync(request.UserId);
            if (userProfile is null)
            {
                return Failure(
                    ResultErrorCodes.Auth_UserNotFound,
                    ("UpdateUserProfile", "Perfil de usuario no encontrado"));
            }

            var result = userProfile.UpdateProfile(request.FirstName, request.LastName, auditContext);
            if (!result.IsSuccess)
            {
                return Failure(
                    result.ErrorCode ?? ResultErrorCodes.GenericError,
                    result.ErrorMessages ?? new[] { ("UpdateUserProfile", "Error al actualizar el perfil") });
            }

            userProfileRepository.Update(userProfile);
            await userProfileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user profile for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.GenericError,
                ("UpdateUserProfile", "Error al actualizar el perfil de usuario"));
        }
    }
}