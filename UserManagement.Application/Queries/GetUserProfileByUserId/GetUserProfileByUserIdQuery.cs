using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Application.DTOs;
using LinaSys.UserManagement.Application.Mappings;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Queries.GetUserProfileByUserId;

public record GetUserProfileByUserIdQuery(string UserId) : IBaseRequest<UserProfileDto>;

public class GetUserProfileByUserIdQueryHandler(
    IUserProfileRepository userProfileRepository,
    ILogger<GetUserProfileByUserIdQueryHandler> logger)
    : BaseCommandHandler<GetUserProfileByUserIdQuery, UserProfileDto>
{
    public override async Task<Result<UserProfileDto>> Handle(GetUserProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userProfile = await userProfileRepository.GetByUserIdAsync(request.UserId);

            if (userProfile is null)
            {
                return Failure(
                    ResultErrorCodes.Auth_UserNotFound,
                    ("GetUserProfileByUserId", "Perfil de usuario no encontrado"));
            }

            var dto = userProfile.ToDto();
            return Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user profile for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.GenericError,
                ("GetUserProfileByUserId", "Error al obtener el perfil de usuario"));
        }
    }
}
