using LinaSys.Auth.Application.Commands;
using LinaSys.Notification.Application.Commands;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Commands;

public record ChangeUserIdentificationOrchestrationCommand(
    string UserId,
    string NewIdentification,
    bool SendNotificationEmail = true) : LinaSys.Shared.Application.MediatR.IBaseRequest;

public class ChangeUserIdentificationOrchestrationCommandHandler(
    IMediator mediator,
    IEmailTemplateService emailTemplateService,
    IConfiguration configuration,
    ITimeProvider timeProvider,
    ILogger<ChangeUserIdentificationOrchestrationCommandHandler> logger)
    : BaseCommandHandler<ChangeUserIdentificationOrchestrationCommand>
{
    public override async Task<Result> Handle(
        ChangeUserIdentificationOrchestrationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Execute the identification change command in Auth domain
            var changeIdentificationCommand = new ChangeUserIdentificationCommand(
                request.UserId,
                request.NewIdentification);

            var changeResult = await mediator.Send(changeIdentificationCommand, cancellationToken);

            if (!changeResult.IsSuccess)
            {
                return Failure(changeResult.ErrorCode ?? ResultErrorCodes.User_IdentificationChangeFailed,
                    ("ChangeIdentification", "Error al cambiar la identificación"));
            }

            // Send notification email if requested and we have the user's email
            if (request.SendNotificationEmail &&
                changeResult.Value?.UserEmail is not null &&
                !string.IsNullOrEmpty(changeResult.Value.UserEmail))
            {
                // Generate email content using template
                var websiteUrl = configuration["Application:WebsiteUrl"] ?? string.Empty;
                var applicationName = configuration["Application:Name"] ?? "Sistema";
                var supportUrl = string.IsNullOrEmpty(websiteUrl) ? string.Empty : $"{websiteUrl}/support";

                var emailContent = emailTemplateService.GenerateIdentificationChangeNotificationEmail(
                    changeResult.Value.NewUserName ?? "Usuario",
                    changeResult.Value.PreviousUserName ?? "Anterior",
                    changeResult.Value.NewUserName ?? request.NewIdentification,
                    timeProvider.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    "Administrador del sistema",
                    supportUrl);

                var emailCommand = new SendEmailCommand(
                    changeResult.Value.UserEmail,
                    $"Tu identificación de usuario en {applicationName} ha sido actualizada",
                    emailContent);
                await mediator.Send(emailCommand, cancellationToken);

                logger.LogInformation(
                    "Identification change notification sent to user {UserId}",
                    request.UserId);
            }

            logger.LogInformation(
                "Identification changed successfully for user {UserId} through orchestration",
                request.UserId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error in identification change orchestration for user {UserId}",
                request.UserId);

            return Failure(ResultErrorCodes.User_IdentificationChangeFailed,
                ("ChangeIdentification", "Error inesperado al cambiar la identificación."));
        }
    }
}