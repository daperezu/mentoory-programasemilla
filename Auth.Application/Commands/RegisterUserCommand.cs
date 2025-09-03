using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Commands;

/// <summary>
/// Command to register a new user.
/// </summary>
/// <param name="IdentificationNumber">The identification number of the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user.</param>
/// <param name="FullName">The full name of the user.</param>
public record RegisterUserCommand(string IdentificationNumber, string Email, string Password, string FullName) : IBaseRequest<User>;

/// <summary>
/// Handler for the RegisterUserCommand.
/// </summary>
public class RegisterUserCommandHandler(
    IAuthRepository authRepository,
    ILogger<RegisterUserCommandHandler> logger)
    : BaseCommandHandler<RegisterUserCommand, User>
{
    /// <summary>
    /// Handles the RegisterUserCommand.
    /// </summary>
    /// <param name="request">The command request containing the identification number, email, password, and full name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the registered user if successful, otherwise an error result.</returns>
    /// <remarks>
    /// Implementation Details:
    /// - Creates a new User object with the provided identification number and email.
    /// - Attempts to create the user using the UserManager with the provided password.
    /// - If user creation is successful, returns a success result with the user.
    /// - If creation fails, logs an error and returns a failure result with the error descriptions.
    /// </remarks>
    public override async Task<Result<User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = request.IdentificationNumber,
            Email = request.Email,
            EmailConfirmed = false,
        };

        var userResult = await authRepository.CreateUserAsync(user, request.Password, cancellationToken);

        if (!userResult.Success)
        {
            logger.LogError("User registration failed for {Email}", request.Email);
            return Failure(ResultErrorCodes.Auth_UserRegistrationFailed, userResult.Errors.Select(s => (nameof(RegisterUserCommand), s)).ToArray());
        }

        logger.LogInformation("User created successfully for {Email}", request.Email);
        return Success(user);
    }
}
