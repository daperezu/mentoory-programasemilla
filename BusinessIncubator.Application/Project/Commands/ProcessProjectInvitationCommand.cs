using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

/// <summary>
/// The action to perform on the invitation.
/// </summary>
public enum InvitationAction
{
    Accept = 1,
    Decline = 2,
    Revoke = 3,
}

/// <summary>
/// Command to process a project invitation (accept, decline, or revoke).
/// </summary>
/// <param name="InvitationToken">The unique invitation token.</param>
/// <param name="Action">The action to perform (Accept, Decline, Revoke).</param>
/// <param name="UserId">The user ID performing the action (required for Accept and Decline).</param>
public record ProcessProjectInvitationCommand(
    string InvitationToken,
    InvitationAction Action,
    string? UserId = null) : IBaseRequest<ProjectInvitationResult>;

/// <summary>
/// Result of processing a project invitation.
/// </summary>
/// <param name="Success">Whether the operation was successful.</param>
/// <param name="InvitationStatus">The current status of the invitation.</param>
/// <param name="ProjectExternalId">The external ID of the project.</param>
/// <param name="ProjectName">The name of the project.</param>
/// <param name="Message">A message describing the result.</param>
public record ProjectInvitationResult(
    bool Success,
    string InvitationStatus,
    Guid? ProjectExternalId = null,
    string? ProjectName = null,
    string? Message = null);

/// <summary>
/// Validator for the ProcessProjectInvitationCommand.
/// </summary>
public class ProcessProjectInvitationCommandValidator : AbstractValidator<ProcessProjectInvitationCommand>
{
    public ProcessProjectInvitationCommandValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty()
            .WithMessage("El token de invitación es requerido.")
            .Length(32, 64)
            .WithMessage("El token de invitación debe tener entre 32 y 64 caracteres.");

        RuleFor(x => x.Action)
            .IsInEnum()
            .WithMessage("La acción especificada no es válida.");

        When(x => x.Action == InvitationAction.Accept || x.Action == InvitationAction.Decline, () =>
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("El ID de usuario es requerido para aceptar o rechazar invitaciones.");
        });
    }
}

/// <summary>
/// Handler for the ProcessProjectInvitationCommand.
/// </summary>
public class ProcessProjectInvitationCommandHandler(
    IBusinessIncubatorRepository businessIncubatorRepository,
    IAuditContext auditContext,
    ILogger<ProcessProjectInvitationCommandHandler> logger)
    : BaseCommandHandler<ProcessProjectInvitationCommand, ProjectInvitationResult>
{
    /// <summary>
    /// Handles the ProcessProjectInvitationCommand.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the invitation processing result.</returns>
    public override async Task<Result<ProjectInvitationResult>> Handle(ProcessProjectInvitationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Find the invitation by token
            var invitation = await FindInvitationByTokenAsync(request.InvitationToken, cancellationToken);
            if (invitation is null)
            {
                logger.LogWarning("Invitation with token {Token} not found", request.InvitationToken);
                return Success(new ProjectInvitationResult(
                    Success: false,
                    InvitationStatus: "NotFound",
                    Message: "La invitación no fue encontrada o ha expirado."));
            }

            // 2. Validate invitation state
            if (invitation.IsExpired(auditContext.UtcNow))
            {
                logger.LogInformation("Attempted to process expired invitation {Token}", request.InvitationToken);
                return Success(new ProjectInvitationResult(
                    Success: false,
                    InvitationStatus: "Expired",
                    ProjectExternalId: await GetProjectExternalIdAsync(invitation.ProjectId, cancellationToken),
                    ProjectName: await GetProjectNameAsync(invitation.ProjectId, cancellationToken),
                    Message: "La invitación ha expirado."));
            }

            if (invitation.Status != Domain.Enums.ProjectInvitationStatus.Pending)
            {
                logger.LogInformation(
                    "Attempted to process non-pending invitation {Token} with status {Status}",
                    request.InvitationToken,
                    invitation.Status);
                return Success(new ProjectInvitationResult(
                    Success: false,
                    InvitationStatus: invitation.Status.ToString(),
                    ProjectExternalId: await GetProjectExternalIdAsync(invitation.ProjectId, cancellationToken),
                    ProjectName: await GetProjectNameAsync(invitation.ProjectId, cancellationToken),
                    Message: $"La invitación ya ha sido {GetStatusMessage(invitation.Status)}."));
            }

            // 3. Process the action
            string resultMessage;
            bool success;

            switch (request.Action)
            {
                case InvitationAction.Accept:
                    success = invitation.Accept(auditContext.UtcNow);
                    resultMessage = success
                        ? "Has aceptado exitosamente la invitación al proyecto."
                        : "No se pudo aceptar la invitación.";

                    if (success)
                    {
                        logger.LogInformation(
                            "User {UserId} accepted invitation {Token} for project {ProjectId}",
                            request.UserId,
                            request.InvitationToken,
                            await GetProjectExternalIdAsync(invitation.ProjectId, cancellationToken));
                    }

                    break;

                case InvitationAction.Decline:
                    success = invitation.Decline(auditContext.UtcNow);
                    resultMessage = success
                        ? "Has rechazado la invitación al proyecto."
                        : "No se pudo rechazar la invitación.";

                    if (success)
                    {
                        logger.LogInformation(
                            "User {UserId} declined invitation {Token} for project {ProjectId}",
                            request.UserId,
                            request.InvitationToken,
                            await GetProjectExternalIdAsync(invitation.ProjectId, cancellationToken));
                    }

                    break;

                case InvitationAction.Revoke:
                    success = invitation.Revoke();
                    resultMessage = success
                        ? "La invitación ha sido revocada exitosamente."
                        : "No se pudo revocar la invitación.";

                    if (success)
                    {
                        logger.LogInformation(
                            "User {UserId} revoked invitation {Token} for project {ProjectId}",
                            auditContext.User,
                            request.InvitationToken,
                            await GetProjectExternalIdAsync(invitation.ProjectId, cancellationToken));
                    }

                    break;

                default:
                    return Failure(
                        ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                        (nameof(request.Action), "Acción no válida."));
            }

            // 4. Save changes
            if (success)
            {
                // Update the invitation
                businessIncubatorRepository.Update(invitation);
            }

            return Success(new ProjectInvitationResult(
                Success: success,
                InvitationStatus: invitation.Status.ToString(),
                ProjectExternalId: await GetProjectExternalIdAsync(invitation.ProjectId, cancellationToken),
                ProjectName: await GetProjectNameAsync(invitation.ProjectId, cancellationToken),
                Message: resultMessage));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing invitation {Token} with action {Action}",
                request.InvitationToken,
                request.Action);

            return Failure(
                ResultErrorCodes.Project_ProcessingFailed,
                (nameof(request), "Error interno al procesar la invitación."));
        }
    }

    private static string GetStatusMessage(Domain.Enums.ProjectInvitationStatus status)
    {
        return status switch
        {
            Domain.Enums.ProjectInvitationStatus.Accepted => "aceptada",
            Domain.Enums.ProjectInvitationStatus.Declined => "rechazada",
            Domain.Enums.ProjectInvitationStatus.Expired => "expirada",
            Domain.Enums.ProjectInvitationStatus.Revoked => "revocada",
            _ => "procesada",
        };
    }

    private async Task<Domain.Aggregates.BusinessIncubator.ProjectInvitation?> FindInvitationByTokenAsync(
        string token, CancellationToken cancellationToken)
    {
        // Get the invitation directly from the repository
        return await businessIncubatorRepository.GetProjectInvitationByTokenAsync(token, cancellationToken);
    }

    private async Task<Guid> GetProjectExternalIdAsync(long projectId, CancellationToken cancellationToken)
    {
        if (projectId == 0)
        {
            return Guid.Empty;
        }

        var project = await businessIncubatorRepository.GetProjectByIdAsync(projectId, cancellationToken);
        return project?.ExternalId ?? Guid.Empty;
    }

    private async Task<string> GetProjectNameAsync(long projectId, CancellationToken cancellationToken)
    {
        if (projectId == 0)
        {
            return string.Empty;
        }

        var project = await businessIncubatorRepository.GetProjectByIdAsync(projectId, cancellationToken);
        return project?.Name ?? string.Empty;
    }
}
