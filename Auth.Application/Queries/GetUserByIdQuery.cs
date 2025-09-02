using FluentValidation;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Queries;

/// <summary>
/// Query to retrieve a user by their ID.
/// </summary>
/// <param name="UserId">The user ID to search for.</param>
public sealed record GetUserByIdQuery(string UserId) : IBaseRequest<UserDetailsResult>;

/// <summary>
/// Result containing user details.
/// </summary>
/// <param name="UserId">The user's ID.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="UserName">The user's username.</param>
/// <param name="FullName">The user's full name.</param>
public sealed record UserDetailsResult(
    string UserId,
    string Email,
    string UserName,
    string? FullName);

/// <summary>
/// Validator for the GetUserByIdQuery.
/// </summary>
public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario no puede estar vacío.")
            .Must(BeValidGuid)
            .WithMessage("El ID del usuario debe ser un GUID válido.");
    }

    private bool BeValidGuid(string userId)
    {
        return Guid.TryParse(userId, out _);
    }
}

/// <summary>
/// Handler for the GetUserByIdQuery.
/// </summary>
public sealed class GetUserByIdQueryHandler(
    IAuthRepository authRepository,
    ILogger<GetUserByIdQueryHandler> logger)
    : BaseCommandHandler<GetUserByIdQuery, UserDetailsResult>
{
    /// <summary>
    /// Handles the GetUserByIdQuery.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the user details.</returns>
    public override async Task<Result<UserDetailsResult>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Searching for user with ID: {UserId}", request.UserId);

            var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("User not found with ID: {UserId}", request.UserId);
                return Failure(
                    ResultErrorCodes.Auth_UserNotFound,
                    (nameof(request.UserId), "Usuario no encontrado."));
            }

            // Construct full name from available user properties
            // Since User doesn't have FirstName/LastName, use UserName or Email
            var fullName = user.UserName ?? user.Email ?? "Usuario";

            var result = new UserDetailsResult(
                UserId: user.Id,
                Email: user.Email ?? string.Empty,
                UserName: user.UserName ?? string.Empty,
                FullName: fullName);
            logger.LogInformation("Successfully retrieved user details for ID: {UserId}", request.UserId);

            return Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by ID: {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.Auth_QueryFailed,
                (nameof(request.UserId), "Error interno al consultar el usuario."));
        }
    }
}