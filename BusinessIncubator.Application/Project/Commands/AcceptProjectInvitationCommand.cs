using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

/// <summary>
/// Command to accept a project invitation and set user password.
/// </summary>
/// <param name="InvitationToken">The unique invitation token.</param>
/// <param name="NewPassword">The new password for the user account.</param>
public record AcceptProjectInvitationCommand(
    string InvitationToken,
    string NewPassword) : IBaseRequest<Result>;

/// <summary>
/// Validator for the AcceptProjectInvitationCommand.
/// </summary>
public class AcceptProjectInvitationCommandValidator : AbstractValidator<AcceptProjectInvitationCommand>
{
    public AcceptProjectInvitationCommandValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty()
            .WithMessage("El token de invitación es requerido.")
            .Length(32, 64)
            .WithMessage("El token de invitación debe tener entre 32 y 64 caracteres.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("La contraseña es requerida.")
            .MinimumLength(6)
            .WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$")
            .WithMessage("La contraseña debe contener al menos: una minúscula, una mayúscula, un número y un carácter especial.");
    }
}

/// <summary>
/// Handler for the AcceptProjectInvitationCommand.
/// </summary>
public class AcceptProjectInvitationCommandHandler(
    IBusinessIncubatorRepository businessIncubatorRepository,
    ITimeProvider timeProvider,
    ILogger<AcceptProjectInvitationCommandHandler> logger)
    : BaseCommandHandler<AcceptProjectInvitationCommand, Result>
{
    /// <summary>
    /// Handles the AcceptProjectInvitationCommand.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public override async Task<Result<Result>> Handle(AcceptProjectInvitationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Find the invitation by token
            var invitation = await businessIncubatorRepository.GetProjectInvitationByTokenAsync(request.InvitationToken, cancellationToken);
            if (invitation is null)
            {
                logger.LogWarning("Invitation with token {Token} not found", request.InvitationToken);
                return Failure(
                    ResultErrorCodes.Unknown,
                    (nameof(request.InvitationToken), "La invitación no fue encontrada."));
            }

            // 2. Validate invitation state
            if (invitation.IsExpired(timeProvider.UtcNow))
            {
                logger.LogInformation("Attempted to accept expired invitation {Token}", request.InvitationToken);
                return Failure(
                    ResultErrorCodes.Unknown,
                    (nameof(request.InvitationToken), "La invitación ha expirado."));
            }

            if (invitation.Status != Domain.Enums.ProjectInvitationStatus.Pending)
            {
                logger.LogInformation(
                    "Attempted to accept non-pending invitation {Token} with status {Status}",
                    request.InvitationToken,
                    invitation.Status);
                return Failure(
                    ResultErrorCodes.Unknown,
                    (nameof(request.InvitationToken), "La invitación ya ha sido procesada."));
            }

            // 3. Accept the invitation (this will create the user account and set permissions)
            var acceptResult = invitation.Accept(timeProvider.UtcNow);
            if (!acceptResult)
            {
                return Failure(
                    ResultErrorCodes.Unknown,
                    (nameof(request.InvitationToken), "No se pudo aceptar la invitación."));
            }

            // 4. Update the invitation in the repository
            // Note: The actual user creation and password setting would be handled
            // by an orchestration command that coordinates between Auth and BusinessIncubator domains
            businessIncubatorRepository.Update(invitation);

            logger.LogInformation(
                "Invitation {Token} accepted successfully for email {Email}",
                request.InvitationToken,
                invitation.Email);

            return Success(Result.Success());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error accepting invitation {Token}", request.InvitationToken);
            return Failure(
                ResultErrorCodes.Project_ProcessingFailed,
                (nameof(request), "Error interno al procesar la invitación."));
        }
    }
}
