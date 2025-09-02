using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.IntegrationEvents;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Commands;

public record GeneratePasswordResetTokenCommand(string UserId) : IBaseRequest<string?>;

public class GeneratePasswordResetTokenCommandHandler(
    IAuthRepository authRepository,
    IPublisher publisher,
    ITimeProvider timeProvider,
    ILogger<GeneratePasswordResetTokenCommandHandler> logger)
    : BaseCommandHandler<GeneratePasswordResetTokenCommand, string?>
{
    public override async Task<Result<string?>> Handle(
        GeneratePasswordResetTokenCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("User {UserId} not found when generating password reset token", request.UserId);
                return Failure(
                    ResultErrorCodes.Auth_UserNotFound,
                    (nameof(request.UserId), $"Usuario con ID {request.UserId} no encontrado."));
            }

            var token = await authRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);

            logger.LogInformation(
                "Password reset token generated for user {UserId}",
                request.UserId);

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
                "Error generating password reset token for user {UserId}",
                request.UserId);

            return Failure(
                ResultErrorCodes.Auth_TokenGenerationFailed,
                ("GenerateToken", "Error al generar el token de restablecimiento."));
        }
    }
}