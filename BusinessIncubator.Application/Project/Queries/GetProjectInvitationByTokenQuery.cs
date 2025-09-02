using LinaSys.BusinessIncubator.Application.Project.DTOs;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.Project.Queries;

public record GetProjectInvitationByTokenQuery(string Token) : IBaseRequest<ProjectInvitationDetailsDto>;

public class GetProjectInvitationByTokenQueryHandler(IBusinessIncubatorRepository repository, ITimeProvider timeProvider)
    : BaseCommandHandler<GetProjectInvitationByTokenQuery, ProjectInvitationDetailsDto>
{
    public override async Task<Result<ProjectInvitationDetailsDto>> Handle(
        GetProjectInvitationByTokenQuery request,
        CancellationToken cancellationToken)
    {
        var invitation = await repository.GetProjectInvitationByTokenAsync(request.Token, cancellationToken);

        if (invitation is null)
        {
            return Failure(ResultErrorCodes.Unknown, (nameof(request.Token), "Invitación no encontrada"));
        }

        // Get project details
        var project = await repository.GetProjectByIdAsync(invitation.ProjectId, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(invitation.ProjectId), "Proyecto no encontrado"));
        }

        var dto = new ProjectInvitationDetailsDto
        {
            Id = invitation.Id,
            ExternalId = invitation.ExternalId,
            ProjectExternalId = project.ExternalId,
            ProjectName = project.Name,
            Email = invitation.Email,
            FullName = invitation.FullName,
            IdentificationNumber = invitation.IdentificationNumber,
            Role = invitation.Role,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            IsExpired = invitation.IsExpired(timeProvider.UtcNow),
        };

        return Success(dto);
    }
}
