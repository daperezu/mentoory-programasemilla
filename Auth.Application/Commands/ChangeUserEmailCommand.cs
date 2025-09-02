using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.IntegrationEvents;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Commands;

public record ChangeUserEmailCommand(string UserId, string NewEmail, bool SkipVerification = false) : IBaseRequest<ChangeUserEmailResult>;

public record ChangeUserEmailResult(string? VerificationToken = null);

public class ChangeUserEmailCommandHandler(
    IAuthRepository authRepository,
    IPublisher publisher,
    ITimeProvider timeProvider,
    ILogger<ChangeUserEmailCommandHandler> logger)
    : BaseCommandHandler<ChangeUserEmailCommand, ChangeUserEmailResult>
{
    public override async Task<Result<ChangeUserEmailResult>> Handle(
        ChangeUserEmailCommand request,
        CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Failure(ResultErrorCodes.Auth_UserNotFound, (nameof(request.UserId), $"Usuario con ID {request.UserId} no encontrado."));
        }

        // Validate new email is not already taken
        var existingUserWithEmail = await authRepository.FindUserByEmailAsync(request.NewEmail, cancellationToken);
        if (existingUserWithEmail is not null && existingUserWithEmail.Id != request.UserId)
        {
            return Failure(ResultErrorCodes.User_EmailAlreadyExists, (nameof(request.NewEmail), $"El correo electrónico {request.NewEmail} ya está registrado."));
        }

        // Check if email is actually changing
        if (user.Email?.Equals(request.NewEmail, StringComparison.OrdinalIgnoreCase) == true)
        {
            return Failure(ResultErrorCodes.User_EmailNotChanged, (nameof(request.NewEmail), "El correo electrónico nuevo es igual al actual."));
        }

        try
        {
            if (request.SkipVerification)
            {
                // Direct email change without verification (for admin operations)
                var token = await authRepository.GenerateChangeEmailTokenAsync(user, request.NewEmail, cancellationToken);
                var result = await authRepository.ChangeEmailAsync(user, request.NewEmail, token, cancellationToken);

                if (!result.Success)
                {
                    var errors = string.Join(", ", result.Errors);
                    return Failure(ResultErrorCodes.User_EmailChangeFailed, ("ChangeEmail", $"Error al cambiar el correo electrónico: {errors}"));
                }

                logger.LogInformation("Email changed directly for user {UserId} to {NewEmail}", request.UserId, request.NewEmail);

                return Success(new ChangeUserEmailResult());
            }

            // Generate email change token for verification
            var verificationToken = await authRepository.GenerateChangeEmailTokenAsync(user, request.NewEmail, cancellationToken);

            logger.LogInformation("Email change token generated for user {UserId} to change to {NewEmail}", request.UserId, request.NewEmail);

            // Publish integration event for email change request
            var integrationEvent = new EmailChangeRequestedIntegrationEvent(
                UserId: user.Id,
                OldEmail: user.Email!,
                NewEmail: request.NewEmail,
                VerificationToken: verificationToken,
                OccurredOn: timeProvider.UtcNow);

            await publisher.Publish(integrationEvent, cancellationToken);
            logger.LogInformation("Published EmailChangeRequestedIntegrationEvent for user {UserId}", user.Id);

            return Success(new ChangeUserEmailResult(verificationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing email for user {UserId} to {NewEmail}", request.UserId, request.NewEmail);

            return Failure(ResultErrorCodes.User_EmailChangeFailed, ("ChangeEmail", "Error inesperado al cambiar el correo electrónico."));
        }
    }
}
