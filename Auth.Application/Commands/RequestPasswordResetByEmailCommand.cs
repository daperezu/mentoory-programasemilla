using LinaSys.Auth.Domain.IntegrationEvents;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Commands;

public record RequestPasswordResetByEmailCommand(string Email) : IBaseRequest<string?>;

public class RequestPasswordResetByEmailCommandHandler(
    IAuthRepository authRepository,
    IPublisher publisher,
    ITimeProvider timeProvider,
    ILogger<RequestPasswordResetByEmailCommandHandler> logger)
    : BaseCommandHandler<RequestPasswordResetByEmailCommand, string?>
{
    public override async Task<Result<string?>> Handle(
        RequestPasswordResetByEmailCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await authRepository.FindUserByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                // Don't reveal if user exists or not for security
                logger.LogWarning("User with email {Email} not found when requesting password reset", request.Email);
                return Success((string?)null);
            }

            // Check if email is confirmed
            if (!await authRepository.IsEmailConfirmedAsync(user, cancellationToken))
            {
                logger.LogWarning("User {UserId} email not confirmed when requesting password reset", user.Id);
                return Success((string?)null);
            }

            var token = await authRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);

            logger.LogInformation(
                "Password reset token generated for user {UserId} with email {Email}",
                user.Id,
                request.Email);

            // Publish integration event for password reset request
            var now = timeProvider.UtcNow;
            var integrationEvent = new PasswordResetRequestedIntegrationEvent(
                UserId: user.Id,
                Email: user.Email!,
                ResetToken: token,
                ExpiresAt: now.AddHours(24), // Standard 24-hour expiry
                OccurredOn: now);

            await publisher.Publish(integrationEvent, cancellationToken);
            logger.LogInformation("Published PasswordResetRequestedIntegrationEvent for user {UserId}", user.Id);

            return Success(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error requesting password reset for email {Email}",
                request.Email);

            return Failure(
                ResultErrorCodes.Auth_TokenGenerationFailed,
                ("RequestPasswordReset", "Error al solicitar el restablecimiento de contraseña."));
        }
    }
}
