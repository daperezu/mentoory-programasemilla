using System.Text;
using LinaSys.Auth.Application.Commands;
using LinaSys.Auth.Application.Queries;
using LinaSys.Notification.Application.Commands;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Commands;

public record ChangeUserEmailOrchestrationCommand(
    string UserId,
    string NewEmail,
    string BaseUrl,
    bool SendVerificationEmail = true) : LinaSys.Shared.Application.MediatR.IBaseRequest;

public class ChangeUserEmailOrchestrationCommandHandler(
    IMediator mediator,
    IEmailTemplateService emailTemplateService,
    ILogger<ChangeUserEmailOrchestrationCommandHandler> logger)
    : BaseCommandHandler<ChangeUserEmailOrchestrationCommand>
{
    public override async Task<Result> Handle(
        ChangeUserEmailOrchestrationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // First, get current user email for the template
            var getUserEmailQuery = new GetUserEmailQuery(request.UserId);
            var currentEmailResult = await mediator.Send(getUserEmailQuery, cancellationToken);
            if (!currentEmailResult.IsSuccess)
            {
                return Failure(ResultErrorCodes.User_NotFound,
                    (nameof(request.UserId), $"Usuario con ID {request.UserId} no encontrado."));
            }

            var currentEmail = currentEmailResult.Value ?? string.Empty;

            // Execute the email change command in Auth domain
            var changeEmailCommand = new ChangeUserEmailCommand(
                request.UserId,
                request.NewEmail,
                !request.SendVerificationEmail);

            var changeResult = await mediator.Send(changeEmailCommand, cancellationToken);

            if (!changeResult.IsSuccess)
            {
                return Failure(changeResult.ErrorCode ?? ResultErrorCodes.User_EmailChangeFailed,
                    ("ChangeEmail", "Error al cambiar el correo electrónico"));
            }

            // If verification is needed, send the verification email
            if (request.SendVerificationEmail && changeResult.Value?.VerificationToken is not null)
            {
                // Encode the token for URL
                var encodedCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(changeResult.Value.VerificationToken))
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", string.Empty);

                // Build confirmation URL
                var callbackUrl = $"{request.BaseUrl}/Account/ConfirmEmailChange?userId={request.UserId}&email={Uri.EscapeDataString(request.NewEmail)}&code={encodedCode}";

                // Generate email content using template
                var emailContent = emailTemplateService.GenerateEmailChangeVerificationEmail(
                    "Usuario", // We don't have the full name in this context
                    currentEmail,
                    request.NewEmail,
                    callbackUrl);

                // Send verification email
                var emailCommand = new SendEmailCommand(
                    request.NewEmail,
                    "Confirma tu nuevo correo electrónico",
                    emailContent);
                await mediator.Send(emailCommand, cancellationToken);

                logger.LogInformation(
                    "Email change verification sent for user {UserId} to {NewEmail}",
                    request.UserId, request.NewEmail);
            }
            else
            {
                logger.LogInformation(
                    "Email changed directly for user {UserId} to {NewEmail}",
                    request.UserId, request.NewEmail);
            }

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error in email change orchestration for user {UserId}",
                request.UserId);

            return Failure(ResultErrorCodes.User_EmailChangeFailed,
                ("ChangeEmail", "Error inesperado al cambiar el correo electrónico."));
        }
    }
}