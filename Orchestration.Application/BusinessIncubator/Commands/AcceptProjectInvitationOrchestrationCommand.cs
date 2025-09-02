using FluentValidation;
using LinaSys.Auth.Application.Commands;
using LinaSys.BusinessIncubator.Application.Project.Commands;
using LinaSys.BusinessIncubator.Application.Project.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using Microsoft.Extensions.Logging;
using IBaseRequest = LinaSys.Shared.Application.MediatR.IBaseRequest;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to orchestrate the complete invitation acceptance process including user creation.
/// </summary>
/// <param name="InvitationToken">The unique invitation token.</param>
/// <param name="NewPassword">The new password for the user account.</param>
public record AcceptProjectInvitationOrchestrationCommand(
    string InvitationToken,
    string NewPassword) : IBaseRequest<Result>;

/// <summary>
/// Validator for the AcceptProjectInvitationOrchestrationCommand.
/// </summary>
public class AcceptProjectInvitationOrchestrationCommandValidator : AbstractValidator<AcceptProjectInvitationOrchestrationCommand>
{
    public AcceptProjectInvitationOrchestrationCommandValidator()
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
/// Handler for the AcceptProjectInvitationOrchestrationCommand.
/// </summary>
public class AcceptProjectInvitationOrchestrationCommandHandler(
    IMediator mediator,
    ILogger<AcceptProjectInvitationOrchestrationCommandHandler> logger)
    : BaseCommandHandler<AcceptProjectInvitationOrchestrationCommand, Result>
{
    /// <summary>
    /// Handles the AcceptProjectInvitationOrchestrationCommand.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public override async Task<Result<Result>> Handle(AcceptProjectInvitationOrchestrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting invitation acceptance orchestration for token {Token}", request.InvitationToken);

            // 1. Get invitation details first
            var invitationQuery = new GetProjectInvitationByTokenQuery(request.InvitationToken);
            var invitationResult = await mediator.Send(invitationQuery, cancellationToken);

            if (!invitationResult.IsSuccess)
            {
                logger.LogWarning("Failed to retrieve invitation details for token {Token}", request.InvitationToken);
                return Failure(
                    ResultErrorCodes.Unknown,
                    (nameof(request.InvitationToken), "La invitación no fue encontrada."));
            }

            var invitation = invitationResult.Value!;

            // 2. Create user account in Auth domain
            var createUserCommand = new CreateUserCommand(
                Email: invitation.Email,
                Password: request.NewPassword,
                Username: !string.IsNullOrWhiteSpace(invitation.IdentificationNumber) ? invitation.IdentificationNumber : null,
                EmailConfirmed: true); // Email is confirmed by accepting the invitation

            var createUserResult = await mediator.Send(createUserCommand, cancellationToken);

            if (!createUserResult.IsSuccess)
            {
                var errorMessage = string.Join("; ", createUserResult.ErrorMessages?.Select(em => em.Message) ?? ["Error al crear la cuenta de usuario"]);
                logger.LogError("Failed to create user account for email {Email} during invitation acceptance: {Error}", invitation.Email, errorMessage);
                return Failure(
                    createUserResult.ErrorCode ?? ResultErrorCodes.Unknown,
                    (nameof(request), errorMessage));
            }

            logger.LogInformation("User account created successfully for email {Email}", invitation.Email);

            // 3. Accept the invitation in BusinessIncubator domain
            var acceptCommand = new AcceptProjectInvitationCommand(
                request.InvitationToken,
                request.NewPassword);

            var acceptResult = await mediator.Send(acceptCommand, cancellationToken);

            if (!acceptResult.IsSuccess)
            {
                logger.LogError("Failed to accept invitation for token {Token} after user creation", request.InvitationToken);

                // TODO: Consider implementing compensation logic to clean up created user
                // For now, we'll return the error but the user account will remain
                return Failure(
                    ResultErrorCodes.Unknown,
                    (nameof(request.InvitationToken), "Error al aceptar la invitación."));
            }

            logger.LogInformation(
                "Invitation acceptance orchestration completed successfully for token {Token} and email {Email}",
                request.InvitationToken,
                invitation.Email);

            return Success(Result.Success());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during invitation acceptance orchestration for token {Token}", request.InvitationToken);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al procesar la invitación."));
        }
    }
}
