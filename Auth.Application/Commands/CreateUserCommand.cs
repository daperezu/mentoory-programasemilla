using FluentValidation;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.IntegrationEvents;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Commands;

/// <summary>
/// Command to create a new user.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user.</param>
/// <param name="Username">The username for the user (optional, defaults to email).</param>
/// <param name="Identification">The user's identification number for login.</param>
/// <param name="EmailConfirmed">Whether the email should be marked as confirmed.</param>
/// <param name="IsTemporaryPassword">Whether the password is system-generated temporary.</param>
public record CreateUserCommand(
    string Email,
    string Password,
    string? Username = null,
    string? Identification = null,
    bool EmailConfirmed = false,
    bool IsTemporaryPassword = false) : IBaseRequest<User>;

/// <summary>
/// Validator for the CreateUserCommand.
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido.")
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido.")
            .MaximumLength(256)
            .WithMessage("El email no puede exceder 256 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");

        When(x => !string.IsNullOrWhiteSpace(x.Username), () =>
        {
            RuleFor(x => x.Username)
                .MaximumLength(256)
                .WithMessage("El nombre de usuario no puede exceder 256 caracteres.");
        });
    }
}

/// <summary>
/// Handler for the CreateUserCommand.
/// </summary>
public class CreateUserCommandHandler(
    IAuthRepository authRepository,
    IPublisher publisher,
    ITimeProvider timeProvider,
    ILogger<CreateUserCommandHandler> logger)
    : BaseCommandHandler<CreateUserCommand, User>
{
    /// <summary>
    /// Handles the CreateUserCommand.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the created user.</returns>
    public override async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user already exists
            var existingUser = await authRepository.FindUserByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null)
            {
                logger.LogWarning("User creation failed: Email {Email} already registered", request.Email);
                return Failure(
                    ResultErrorCodes.Auth_UserRegistrationFailed,
                    (nameof(request.Email), "El email ya está registrado."));
            }

            // Create new user
            var username = !string.IsNullOrWhiteSpace(request.Username)
                ? request.Username
                : request.Email;

            var user = new User
            {
                UserName = username,
                Email = request.Email,
                EmailConfirmed = request.EmailConfirmed,
            };

            logger.LogInformation("Attempting to create user with UserName: {UserName}, Email: {Email}", username, request.Email);

            var userResult = await authRepository.CreateUserAsync(user, request.Password, cancellationToken);

            if (!userResult.Success)
            {
                var errorMessage = string.Join("; ", userResult.Errors);
                logger.LogError("User creation failed for {Email}: {Errors}", request.Email, errorMessage);
                return Failure(
                    ResultErrorCodes.Auth_UserRegistrationFailed,
                    (nameof(request.Email), errorMessage));
            }

            logger.LogInformation("User created successfully for {Email} with UserName: {UserName}, UserId: {UserId}",
                request.Email, user.UserName, user.Id);

            // NOTE: Default role assignment removed - roles are now assigned explicitly during user creation process
            // The calling code (e.g., CreateUserWithProfileOrchestrationCommand) is responsible for assigning the appropriate role

            // Generate email confirmation token if email is not confirmed
            string? emailConfirmationToken = null;
            if (!request.EmailConfirmed)
            {
                emailConfirmationToken = await authRepository.GenerateEmailConfirmationTokenAsync(user, cancellationToken);
                logger.LogInformation("Generated email confirmation token for user {UserId}", user.Id);
            }

            // Publish integration event for user account creation
            var now = timeProvider.UtcNow;
            var integrationEvent = new UserAccountCreatedIntegrationEvent(
                UserId: user.Id,
                Email: user.Email!,
                Identification: request.Identification,
                TemporaryPassword: request.Password,
                CreatedAt: now,
                OccurredOn: now,
                EmailConfirmed: request.EmailConfirmed,
                IsTemporaryPassword: request.IsTemporaryPassword,
                EmailConfirmationToken: emailConfirmationToken);

            await publisher.Publish(integrationEvent, cancellationToken);
            logger.LogInformation("Published UserAccountCreatedIntegrationEvent for user {UserId}", user.Id);

            return Success(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating user {Email}", request.Email);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request.Email), "Error interno durante la creación del usuario."));
        }
    }
}
