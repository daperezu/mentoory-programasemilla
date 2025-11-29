using FluentValidation;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Commands;

/// <summary>
/// Command to deactivate a user's access to a project.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="ProjectId">The project identifier.</param>
public sealed record DeactivateProjectAccessCommand(string UserId, long ProjectId) : IBaseRequest;

/// <summary>
/// Validator for DeactivateProjectAccessCommand.
/// </summary>
public sealed class DeactivateProjectAccessCommandValidator : AbstractValidator<DeactivateProjectAccessCommand>
{
    public DeactivateProjectAccessCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario no puede estar vacío.")
            .Must(BeValidGuid)
            .WithMessage("El ID del usuario debe ser un GUID válido.");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("El ID del proyecto debe ser mayor que cero.");
    }

    private bool BeValidGuid(string userId)
    {
        return Guid.TryParse(userId, out _);
    }
}

/// <summary>
/// Handler for DeactivateProjectAccessCommand.
/// </summary>
public sealed class DeactivateProjectAccessCommandHandler(
    IAuthRepository authRepository,
    ITimeProvider timeProvider,
    ILogger<DeactivateProjectAccessCommandHandler> logger) : BaseCommandHandler<DeactivateProjectAccessCommand>
{
    public override async Task<Result> Handle(DeactivateProjectAccessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deactivating project access for user {UserId} from project {ProjectId}",
                request.UserId, request.ProjectId);

            // Get existing access record
            var access = await authRepository.GetUserProjectAccessAsync(
                request.UserId,
                request.ProjectId,
                cancellationToken);

            if (access is null)
            {
                logger.LogWarning("Project access not found for user {UserId} and project {ProjectId}",
                    request.UserId, request.ProjectId);
                return Failure(
                    ResultErrorCodes.Auth_QueryFailed,
                    (nameof(request.ProjectId), "No se encontró el acceso del usuario al proyecto."));
            }

            if (!access.IsActive)
            {
                // Already deactivated
                logger.LogInformation("Project access already deactivated for user {UserId} and project {ProjectId}",
                    request.UserId, request.ProjectId);
                return Success();
            }

            // Deactivate the access
            var currentTime = timeProvider.UtcNow;
            access.Deactivate(currentTime);

            // Update in repository
            await authRepository.UpdateUserProjectAccessAsync(access, cancellationToken);

            // Save changes
            await authRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            logger.LogInformation("Successfully deactivated project access for user {UserId} from project {ProjectId}",
                request.UserId, request.ProjectId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating project access for user {UserId} from project {ProjectId}",
                request.UserId, request.ProjectId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(request.ProjectId), "Error interno al desactivar el acceso al proyecto."));
        }
    }
}
