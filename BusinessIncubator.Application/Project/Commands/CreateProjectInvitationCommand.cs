using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

/// <summary>
/// Command to create a project invitation for a user.
/// </summary>
/// <param name="ProjectExternalId">The external ID of the project.</param>
/// <param name="Email">The email of the user to invite.</param>
/// <param name="FullName">The full name of the user to invite.</param>
/// <param name="IdentificationNumber">The identification number of the user.</param>
/// <param name="Role">The role ID to assign to the user in the project.</param>
/// <param name="ExpirationDays">The number of days until the invitation expires.</param>
public record CreateProjectInvitationCommand(
    Guid ProjectExternalId,
    string Email,
    string FullName,
    string IdentificationNumber,
    string? Role,
    int ExpirationDays = 7) : IBaseRequest<string>;

/// <summary>
/// Validator for the CreateProjectInvitationCommand.
/// </summary>
public class CreateProjectInvitationCommandValidator : AbstractValidator<CreateProjectInvitationCommand>
{
    public CreateProjectInvitationCommandValidator()
    {
        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido.")
            .EmailAddress()
            .WithMessage("El formato del email es inválido.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("El nombre completo es requerido.");

        RuleFor(x => x.IdentificationNumber)
            .NotEmpty()
            .WithMessage("El número de identificación es requerido.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.Role))
            .WithMessage("El rol no puede estar vacío si se proporciona.");

        RuleFor(x => x.ExpirationDays)
            .GreaterThan(0)
            .WithMessage("Los días de expiración deben ser mayor a cero.")
            .LessThanOrEqualTo(365)
            .WithMessage("Los días de expiración no pueden exceder un año.");
    }
}

/// <summary>
/// Handler for the CreateProjectInvitationCommand.
/// </summary>
public class CreateProjectInvitationCommandHandler(
    IBusinessIncubatorRepository businessIncubatorRepository,
    IAuditContext auditContext,
    ILogger<CreateProjectInvitationCommandHandler> logger)
    : BaseCommandHandler<CreateProjectInvitationCommand, string>
{
    /// <summary>
    /// Handles the CreateProjectInvitationCommand.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the invitation token.</returns>
    public override async Task<Result<string>> Handle(CreateProjectInvitationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get the project
            var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
            if (project is null)
            {
                logger.LogWarning("Project with external ID {ProjectId} not found", request.ProjectExternalId);
                return Failure(
                    ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "El proyecto no fue encontrado."));
            }

            // 2. Create the invitation
            var invitation = project.CreateInvitation(
                request.Email,
                request.FullName,
                request.IdentificationNumber,
                request.Role,
                request.ExpirationDays,
                auditContext);

            // 3. Save changes
            businessIncubatorRepository.Update(project);

            logger.LogInformation(
                "Created invitation for {Email} to project {ProjectId} with token {Token}",
                request.Email,
                request.ProjectExternalId,
                invitation.InvitationToken);

            return Success(invitation.InvitationToken);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already a pending invitation"))
        {
            logger.LogWarning("Duplicate invitation attempt for {Email} in project {ProjectId}", request.Email, request.ProjectExternalId);
            return Failure(ResultErrorCodes.Project_DuplicateInvitation, (nameof(request.Email), "Ya existe una invitación pendiente para este correo electrónico."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating invitation for {Email} to project {ProjectId}", request.Email, request.ProjectExternalId);
            return Failure(ResultErrorCodes.Unknown, (nameof(request), "Error interno al crear la invitación."));
        }
    }
}
