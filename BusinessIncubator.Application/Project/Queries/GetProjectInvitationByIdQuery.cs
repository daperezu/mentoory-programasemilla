using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Queries;

/// <summary>
/// Query to get a project invitation by its ID.
/// </summary>
/// <param name="ProjectExternalId">The external ID of the project.</param>
/// <param name="InvitationExternalId">The external ID of the invitation.</param>
public record GetProjectInvitationByIdQuery(
    Guid ProjectExternalId,
    Guid InvitationExternalId) : IBaseRequest<ProjectInvitationDto?>;

/// <summary>
/// DTO for project invitation.
/// </summary>
/// <param name="Id">The invitation ID.</param>
/// <param name="Email">The invited user's email.</param>
/// <param name="FullName">The invited user's full name.</param>
/// <param name="Role">The role assigned to the user.</param>
/// <param name="Status">The current invitation status.</param>
/// <param name="InvitationToken">The invitation token.</param>
/// <param name="CreatedAt">When the invitation was created.</param>
/// <param name="ExpiresAt">When the invitation expires.</param>
public record ProjectInvitationDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string Status,
    string InvitationToken,
    DateTime CreatedAt,
    DateTime? ExpiresAt);

/// <summary>
/// Validator for the GetProjectInvitationByIdQuery.
/// </summary>
public class GetProjectInvitationByIdQueryValidator : AbstractValidator<GetProjectInvitationByIdQuery>
{
    public GetProjectInvitationByIdQueryValidator()
    {
        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.InvitationExternalId)
            .NotEmpty()
            .WithMessage("El ID de la invitación es requerido.");
    }
}

/// <summary>
/// Handler for the GetProjectInvitationByIdQuery.
/// </summary>
public class GetProjectInvitationByIdQueryHandler(
    IBusinessIncubatorRepository businessIncubatorRepository,
    ILogger<GetProjectInvitationByIdQueryHandler> logger)
    : BaseCommandHandler<GetProjectInvitationByIdQuery, ProjectInvitationDto?>
{
    /// <summary>
    /// Handles the GetProjectInvitationByIdQuery.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the invitation DTO or null if not found.</returns>
    public override async Task<Result<ProjectInvitationDto?>> Handle(
        GetProjectInvitationByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project with its invitations
            var project = await businessIncubatorRepository
                .GetProjectWithInvitationsByExternalIdAsync(request.ProjectExternalId, cancellationToken);

            if (project is null)
            {
                logger.LogWarning("Project with external ID {ProjectId} not found", request.ProjectExternalId);
                return Success(null as ProjectInvitationDto);
            }

            var invitation = project.ProjectInvitations?
                .FirstOrDefault(i => i.ExternalId == request.InvitationExternalId);

            if (invitation is null)
            {
                logger.LogWarning(
                    "Invitation with external ID {InvitationId} not found in project {ProjectId}",
                    request.InvitationExternalId,
                    request.ProjectExternalId);
                return Success(null as ProjectInvitationDto);
            }

            var dto = new ProjectInvitationDto(
                Id: invitation.ExternalId,
                Email: invitation.Email,
                FullName: invitation.FullName,
                Role: invitation.Role,
                Status: invitation.Status.ToString(),
                InvitationToken: invitation.InvitationToken,
                CreatedAt: invitation.CreatedAt,
                ExpiresAt: invitation.ExpiresAt);

            logger.LogInformation(
                "Retrieved invitation {InvitationId} from project {ProjectId}",
                request.InvitationExternalId,
                request.ProjectExternalId);

            return Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error retrieving invitation {InvitationId} from project {ProjectId}",
                request.InvitationExternalId,
                request.ProjectExternalId);

            return Failure(
                ResultErrorCodes.Project_ProcessingFailed,
                (nameof(request), "Error interno al obtener la invitación."));
        }
    }
}
