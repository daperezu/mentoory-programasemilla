using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.UserManagement.Domain.AggregatesModel.UserProfileAggregate;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Commands.CreateUserProfile;

public class CreateUserProfileCommandHandler(
    IUserProfileRepository userProfileRepository,
    IAuditContext auditContext,
    ILogger<CreateUserProfileCommandHandler> logger)
    : BaseCommandHandler<CreateUserProfileCommand, int>
{
    public override async Task<Result<int>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user already has a profile
            if (await userProfileRepository.ExistsAsync(request.UserId))
            {
                return Failure(
                    ResultErrorCodes.ValidationError,
                    ("CreateUserProfile", "El usuario ya tiene un perfil creado"));
            }

            // Check if identification already exists
            if (await userProfileRepository.IdentificationExistsAsync(request.Identification))
            {
                return Failure(
                    ResultErrorCodes.ValidationError,
                    ("CreateUserProfile", "La identificación ya está registrada"));
            }

            // Create the user profile
            var userProfile = UserProfile.Create(
                request.UserId,
                request.FirstName,
                request.LastName,
                request.Identification,
                auditContext);

            userProfileRepository.Add(userProfile);
            await userProfileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            logger.LogInformation(
                "User profile created successfully for user {UserId} with ID {UserProfileId}",
                request.UserId,
                userProfile.Id);

            return Success((int)userProfile.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user profile for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.GenericError,
                ("CreateUserProfile", "Error al crear el perfil de usuario"));
        }
    }
}