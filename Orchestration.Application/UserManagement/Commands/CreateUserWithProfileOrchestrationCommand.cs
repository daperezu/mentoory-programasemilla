using FluentValidation;
using LinaSys.Auth.Application.Commands;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Commands;

public record CreateUserWithProfileOrchestrationCommand(
    string Email,
    string FirstName,
    string LastName,
    string Identification,
    string Password,
    string? Country,
    string? Province,
    string? Canton,
    string? District,
    string? FullAddress,
    Dictionary<string, string>? EmailPreferences = null,
    bool EmailConfirmed = false,
    bool IsTemporaryPassword = false) : IBaseRequest<string>;

public class CreateUserWithProfileOrchestrationCommandValidator : AbstractValidator<CreateUserWithProfileOrchestrationCommand>
{
    public CreateUserWithProfileOrchestrationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres.");

        RuleFor(x => x.Identification)
            .NotEmpty().WithMessage("La identificación es requerida.")
            .MaximumLength(50).WithMessage("La identificación no puede exceder 50 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$")
            .WithMessage("La contraseña debe contener al menos: una minúscula, una mayúscula, un número y un carácter especial.");

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

public class CreateUserWithProfileOrchestrationCommandHandler(
    IMediator mediator,
    ILogger<CreateUserWithProfileOrchestrationCommandHandler> logger)
    : BaseCommandHandler<CreateUserWithProfileOrchestrationCommand, string>
{
    public override async Task<Result<string>> Handle(CreateUserWithProfileOrchestrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting user creation orchestration for email {Email}", request.Email);

            // 1. Create user in Auth domain
            var createUserCommand = new CreateUserCommand(
                Email: request.Email,
                Password: request.Password,
                Username: request.Identification,
                Identification: request.Identification,
                EmailConfirmed: request.EmailConfirmed,
                IsTemporaryPassword: request.IsTemporaryPassword);

            var createUserResult = await mediator.Send(createUserCommand, cancellationToken);

            if (!createUserResult.IsSuccess)
            {
                var errorMessage = string.Join("; ", createUserResult.ErrorMessages?.Select(em => em.Message) ?? ["Error al crear la cuenta de usuario"]);
                logger.LogError("Failed to create user account for email {Email}: {Error}", request.Email, errorMessage);
                return Failure(
                    createUserResult.ErrorCode ?? ResultErrorCodes.Unknown,
                    (nameof(request.Email), errorMessage));
            }

            var user = createUserResult.Value!;
            var userId = user.Id;
            logger.LogInformation("User account created successfully with ID {UserId}", userId);

            // 2. Create UserProfile in UserManagement domain
            var createProfileCommand = new LinaSys.UserManagement.Application.Commands.CreateUserProfile.CreateUserProfileCommand(
                UserId: userId,
                FirstName: request.FirstName,
                LastName: request.LastName,
                Identification: request.Identification,
                Email: request.Email);

            var createProfileResult = await mediator.Send(createProfileCommand, cancellationToken);

            if (!createProfileResult.IsSuccess)
            {
                logger.LogError("Failed to create user profile for user {UserId}", userId);
                // TODO: Consider implementing compensation logic to delete created user
                return Failure(
                    createProfileResult.ErrorCode ?? ResultErrorCodes.Unknown,
                    (nameof(request), "Error al crear el perfil de usuario."));
            }

            var profileId = createProfileResult.Value;
            logger.LogInformation("User profile created successfully with ID {ProfileId}", profileId);

            // 3. Update location if provided
            if (!string.IsNullOrWhiteSpace(request.Country))
            {
                var updateLocationCommand = new LinaSys.UserManagement.Application.Commands.UpdateUserLocation.UpdateUserLocationCommand(
                    UserId: userId,
                    Country: request.Country,
                    Province: request.Province,
                    Canton: request.Canton,
                    District: request.District,
                    FullAddress: request.FullAddress);

                var locationResult = await mediator.Send(updateLocationCommand, cancellationToken);

                if (!locationResult.IsSuccess)
                {
                    logger.LogWarning("Failed to update location for user {UserId}, but continuing", userId);
                }
            }

            // 4. Update all preferences (email preferences + temporary password flag) in a single call
            var allPreferences = new Dictionary<string, string>(request.EmailPreferences ?? new Dictionary<string, string>());

            // Add temporary password preference if applicable
            if (request.IsTemporaryPassword)
            {
                allPreferences["auth.requires_password_change"] = "true";
            }

            // Only send preferences update if there are any preferences to set
            if (allPreferences.Count > 0)
            {
                var updatePreferencesCommand = new LinaSys.UserManagement.Application.Commands.UpdateUserPreferences.UpdateUserPreferencesCommand(
                    UserId: userId,
                    Preferences: allPreferences);

                var preferencesResult = await mediator.Send(updatePreferencesCommand, cancellationToken);

                if (!preferencesResult.IsSuccess)
                {
                    logger.LogWarning("Failed to update preferences for user {UserId}, but continuing", userId);
                    // Not failing the entire operation for preference update failure
                }
                else
                {
                    logger.LogInformation("Updated {Count} preferences for user {UserId}",
                        allPreferences.Count, userId);
                }
            }

            logger.LogInformation(
                "User creation orchestration completed successfully for email {Email} with user ID {UserId}",
                request.Email,
                userId);

            return Success(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user creation orchestration for email {Email}", request.Email);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al crear el usuario."));
        }
    }
}
