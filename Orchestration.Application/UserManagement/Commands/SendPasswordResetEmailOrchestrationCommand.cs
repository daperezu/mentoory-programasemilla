using System.Text;
using LinaSys.Auth.Application.Commands;
using LinaSys.Auth.Application.Queries;
using LinaSys.Notification.Application.Commands;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.UserManagement.Application.Queries.GetUserProfileByUserId;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Commands;

public record SendPasswordResetEmailOrchestrationCommand(
    string UserId,
    string BaseUrl,
    string? RequestLocation = null) : LinaSys.Shared.Application.MediatR.IBaseRequest;

public class SendPasswordResetEmailOrchestrationCommandHandler(
    IMediator mediator,
    IEmailTemplateService emailTemplateService,
    IConfiguration configuration,
    ITimeProvider timeProvider,
    ILogger<SendPasswordResetEmailOrchestrationCommandHandler> logger)
    : BaseCommandHandler<SendPasswordResetEmailOrchestrationCommand>
{
    public override async Task<Result> Handle(
        SendPasswordResetEmailOrchestrationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get user email from Auth domain
            var getUserEmailQuery = new GetUserEmailQuery(request.UserId);
            var emailResult = await mediator.Send(getUserEmailQuery, cancellationToken);

            if (!emailResult.IsSuccess || string.IsNullOrEmpty(emailResult.Value))
            {
                return Failure(ResultErrorCodes.User_NotFound,
                    (nameof(request.UserId), $"Usuario con ID {request.UserId} no encontrado o sin correo electrónico."));
            }

            // Get user profile for the full name
            var getUserProfileQuery = new GetUserProfileByUserIdQuery(request.UserId);
            var profileResult = await mediator.Send(getUserProfileQuery, cancellationToken);

            var fullName = profileResult.IsSuccess && profileResult.Value != null
                ? $"{profileResult.Value.FirstName} {profileResult.Value.LastName}"
                : "Usuario";

            // Generate password reset token using Auth domain command
            var generateTokenCommand = new GeneratePasswordResetTokenCommand(request.UserId);
            var tokenResult = await mediator.Send(generateTokenCommand, cancellationToken);

            if (!tokenResult.IsSuccess || string.IsNullOrEmpty(tokenResult.Value))
            {
                return Failure(ResultErrorCodes.Auth_TokenGenerationFailed,
                    ("GenerateToken", "Error al generar el token de restablecimiento de contraseña."));
            }

            // Encode the token for URL
            var encodedCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenResult.Value))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", string.Empty);

            // Build reset URL
            var resetUrl = $"{request.BaseUrl}/Account/ResetPassword?userId={request.UserId}&code={encodedCode}";

            // Get request details
            var requestDateTime = timeProvider.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var requestLocation = request.RequestLocation ?? "Ubicación desconocida";

            // Generate password reset email content using template
            var emailContent = emailTemplateService.GeneratePasswordResetEmail(
                fullName,
                emailResult.Value,
                resetUrl,
                requestDateTime,
                requestLocation);

            // Get application name from configuration
            var applicationName = configuration["Application:Name"] ?? "Sistema";

            // Send password reset email
            var emailCommand = new SendEmailCommand(
                emailResult.Value,
                $"Restablecer tu contraseña - {applicationName}",
                emailContent);

            await mediator.Send(emailCommand, cancellationToken);

            logger.LogInformation(
                "Password reset email sent to user {UserId} at {Email}",
                request.UserId, emailResult.Value);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error sending password reset email to user {UserId}",
                request.UserId);

            return Failure(ResultErrorCodes.Notification_SendFailed,
                ("SendPasswordResetEmail", "Error inesperado al enviar el correo de restablecimiento de contraseña."));
        }
    }
}