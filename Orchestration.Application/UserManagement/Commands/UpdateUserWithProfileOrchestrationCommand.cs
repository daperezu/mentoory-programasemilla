using FluentValidation;
using LinaSys.Auth.Application.Commands;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Application.Commands.UpdateUserProfile;
using LinaSys.UserManagement.Application.Commands.UpdateUserLocation;
using LinaSys.UserManagement.Application.Queries.GetUserProfileByUserId;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Commands;

public record UpdateUserWithProfileOrchestrationCommand(
    string UserId,
    string FirstName,
    string LastName,
    string? Country,
    string? Province,
    string? Canton,
    string? District,
    string? FullAddress) : LinaSys.Shared.Application.MediatR.IBaseRequest;

public class UpdateUserWithProfileOrchestrationCommandValidator : AbstractValidator<UpdateUserWithProfileOrchestrationCommand>
{
    public UpdateUserWithProfileOrchestrationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres.");

        When(x => x.Country?.Equals("Costa Rica", StringComparison.OrdinalIgnoreCase) == true, () =>
        {
            RuleFor(x => x.Province)
                .NotEmpty().WithMessage("La provincia es requerida para Costa Rica.");
            RuleFor(x => x.Canton)
                .NotEmpty().WithMessage("El cantón es requerido para Costa Rica.");
            RuleFor(x => x.District)
                .NotEmpty().WithMessage("El distrito es requerido para Costa Rica.");
        });
    }
}

public class UpdateUserWithProfileOrchestrationCommandHandler(
    IMediator mediator,
    ILogger<UpdateUserWithProfileOrchestrationCommandHandler> logger)
    : BaseCommandHandler<UpdateUserWithProfileOrchestrationCommand>
{
    public override async Task<Result> Handle(UpdateUserWithProfileOrchestrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting user update orchestration for user {UserId}", request.UserId);

            // 1. Verify user profile exists
            var getProfileQuery = new GetUserProfileByUserIdQuery(request.UserId);
            var profileResult = await mediator.Send(getProfileQuery, cancellationToken);

            if (!profileResult.IsSuccess)
            {
                logger.LogWarning("User profile not found for user {UserId}", request.UserId);
                return Failure(
                    ResultErrorCodes.Auth_UserNotFound,
                    (nameof(request.UserId), "El perfil de usuario no fue encontrado."));
            }

            // 2. Update user profile in UserManagement domain
            var updateProfileCommand = new UpdateUserProfileCommand(
                UserId: request.UserId,
                FirstName: request.FirstName,
                LastName: request.LastName);

            var updateProfileResult = await mediator.Send(updateProfileCommand, cancellationToken);

            if (!updateProfileResult.IsSuccess)
            {
                logger.LogError("Failed to update user profile for user {UserId}", request.UserId);
                return Failure(
                    updateProfileResult.ErrorCode ?? ResultErrorCodes.Unknown,
                    (nameof(request), "Error al actualizar el perfil de usuario."));
            }

            logger.LogInformation("User profile updated successfully for user {UserId}", request.UserId);

            // 3. Update location if provided
            if (!string.IsNullOrWhiteSpace(request.Country))
            {
                var updateLocationCommand = new UpdateUserLocationCommand(
                    UserId: request.UserId,
                    Country: request.Country,
                    Province: request.Province,
                    Canton: request.Canton,
                    District: request.District,
                    FullAddress: request.FullAddress);

                var locationResult = await mediator.Send(updateLocationCommand, cancellationToken);

                if (!locationResult.IsSuccess)
                {
                    logger.LogWarning("Failed to update location for user {UserId}", request.UserId);
                    return Failure(
                        locationResult.ErrorCode ?? ResultErrorCodes.Unknown,
                        (nameof(request), "Error al actualizar la ubicación."));
                }
            }

            logger.LogInformation(
                "User update orchestration completed successfully for user {UserId}",
                request.UserId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user update orchestration for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al actualizar el usuario."));
        }
    }
}