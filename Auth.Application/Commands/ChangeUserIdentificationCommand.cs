using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Commands;

public record ChangeUserIdentificationCommand(string UserId, string NewIdentification) : IBaseRequest<ChangeUserIdentificationResult>;

public record ChangeUserIdentificationResult(
    string? UserEmail = null,
    string? PreviousUserName = null,
    string? NewUserName = null);

public class ChangeUserIdentificationCommandHandler(IAuthRepository authRepository, ILogger<ChangeUserIdentificationCommandHandler> logger)
    : BaseCommandHandler<ChangeUserIdentificationCommand, ChangeUserIdentificationResult>
{
    public override async Task<Result<ChangeUserIdentificationResult>> Handle(ChangeUserIdentificationCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Failure(ResultErrorCodes.Auth_UserNotFound, (nameof(request.UserId), $"Usuario con ID {request.UserId} no encontrado."));
        }

        // Validate new identification format (numeric only, as per Identity configuration)
        if (string.IsNullOrWhiteSpace(request.NewIdentification) ||
            !System.Text.RegularExpressions.Regex.IsMatch(request.NewIdentification, @"^[0-9]+$"))
        {
            return Failure(ResultErrorCodes.User_InvalidIdentification, (nameof(request.NewIdentification), "La identificación debe contener solo números."));
        }

        // Check if identification is not already taken
        var existingUser = await authRepository.FindUserByNameAsync(request.NewIdentification, cancellationToken);
        if (existingUser != null && existingUser.Id != request.UserId)
        {
            return Failure(ResultErrorCodes.User_IdentificationAlreadyExists, (nameof(request.NewIdentification), $"La identificación {request.NewIdentification} ya está en uso."));
        }

        // Check if it's actually changing
        if (user.UserName?.Equals(request.NewIdentification, StringComparison.OrdinalIgnoreCase) == true)
        {
            return Failure(ResultErrorCodes.User_IdentificationNotChanged, (nameof(request.NewIdentification), "La nueva identificación es igual a la actual."));
        }

        try
        {
            var previousUserName = user.UserName;

            // Change the username
            var result = await authRepository.SetUserNameAsync(user, request.NewIdentification, cancellationToken);

            if (!result.Success)
            {
                var errors = string.Join(", ", result.Errors);
                return Failure(
                    ResultErrorCodes.User_IdentificationChangeFailed,
                    ("ChangeIdentification", $"Error al cambiar la identificación: {errors}"));
            }

            logger.LogInformation(
                "Identification changed successfully for user {UserId} from {OldIdentification} to {NewIdentification}",
                request.UserId, previousUserName, request.NewIdentification);

            return Success(new ChangeUserIdentificationResult(
                user.Email,
                previousUserName,
                request.NewIdentification));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error changing identification for user {UserId}",
                request.UserId);

            return Failure(
                ResultErrorCodes.User_IdentificationChangeFailed,
                ("ChangeIdentification", "Error inesperado al cambiar la identificación."));
        }
    }
}
