using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Commands.UpdateUserPreferences;

public record UpdateUserPreferencesCommand(
    string UserId,
    Dictionary<string, string> Preferences) : IBaseRequest;

public class UpdateUserPreferencesCommandValidator : AbstractValidator<UpdateUserPreferencesCommand>
{
    public UpdateUserPreferencesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido.");

        RuleFor(x => x.Preferences)
            .NotNull().WithMessage("Las preferencias son requeridas.");
    }
}

public class UpdateUserPreferencesCommandHandler(
    IUserProfileRepository userProfileRepository,
    IAuditContext auditContext,
    ILogger<UpdateUserPreferencesCommandHandler> logger)
    : BaseCommandHandler<UpdateUserPreferencesCommand>
{
    public override async Task<Result> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userProfile = await userProfileRepository.GetByUserIdAsync(request.UserId);
            if (userProfile is null)
            {
                logger.LogWarning("User profile not found for user ID {UserId}", request.UserId);
                return Failure(
                    ResultErrorCodes.Auth_UserNotFound,
                    (nameof(request.UserId), "Perfil de usuario no encontrado."));
            }

            // Update each preference
            foreach (var preference in request.Preferences)
            {
                var updateResult = userProfile.AddOrUpdatePreference(
                    preference.Key,
                    preference.Value,
                    auditContext);

                if (!updateResult.IsSuccess)
                {
                    logger.LogWarning("Failed to update preference {Key} for user {UserId}",
                        preference.Key, request.UserId);
                    return Failure(
                        updateResult.ErrorCode ?? ResultErrorCodes.ValidationError,
                        (preference.Key, $"Error al actualizar la preferencia {preference.Key}"));
                }
            }

            userProfileRepository.Update(userProfile);
            await userProfileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully updated {Count} preferences for user {UserId}",
                request.Preferences.Count,
                request.UserId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating preferences for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al actualizar las preferencias."));
        }
    }
}