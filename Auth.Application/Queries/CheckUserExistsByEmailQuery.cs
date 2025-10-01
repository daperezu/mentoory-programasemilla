using FluentValidation;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Queries;

/// <summary>
/// Query to check if a user exists by email.
/// </summary>
/// <param name="Email">The email to check.</param>
public record CheckUserExistsByEmailQuery(string Email) : IBaseRequest<UserExistsResult>;

/// <summary>
/// Result indicating whether a user exists.
/// </summary>
/// <param name="Exists">Whether the user exists.</param>
/// <param name="UserId">The user ID if the user exists.</param>
public record UserExistsResult(bool Exists, string? UserId = null);

/// <summary>
/// Validator for the CheckUserExistsByEmailQuery.
/// </summary>
public class CheckUserExistsByEmailQueryValidator : AbstractValidator<CheckUserExistsByEmailQuery>
{
    public CheckUserExistsByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido.")
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido.");
    }
}

/// <summary>
/// Handler for the CheckUserExistsByEmailQuery.
/// </summary>
public class CheckUserExistsByEmailQueryHandler(
    IAuthRepository authRepository,
    ILogger<CheckUserExistsByEmailQueryHandler> logger)
    : BaseCommandHandler<CheckUserExistsByEmailQuery, UserExistsResult>
{
    public override async Task<Result<UserExistsResult>> Handle(CheckUserExistsByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await authRepository.FindUserByEmailAsync(request.Email, cancellationToken);

            if (user is not null)
            {
                logger.LogInformation("User found with email {Email}", request.Email);
                return Success(new UserExistsResult(true, user.Id));
            }

            logger.LogInformation("No user found with email {Email}", request.Email);
            return Success(new UserExistsResult(false));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if user exists with email {Email}", request.Email);
            return Failure(
                ResultErrorCodes.Auth_QueryFailed,
                (nameof(request.Email), "Error al verificar si el usuario existe."));
        }
    }
}
