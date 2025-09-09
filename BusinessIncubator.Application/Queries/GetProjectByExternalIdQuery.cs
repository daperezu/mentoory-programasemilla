using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

public sealed record GetProjectByExternalIdQuery(Guid ExternalId, string? CheckAccessForUserId = null) : IBaseRequest<ProjectByExternalIdDto>;

public sealed record ProjectByExternalIdDto(
    long Id,
    Guid ExternalId,
    string Name,
    string? Description,
    long IncubatorId,
    bool IsActive,
    bool? HasAccess = null);

public sealed class GetProjectByExternalIdQueryHandler(
    IBusinessIncubatorRepository repository) : BaseCommandHandler<GetProjectByExternalIdQuery, ProjectByExternalIdDto>
{
    public override async Task<Result<ProjectByExternalIdDto>> Handle(GetProjectByExternalIdQuery request, CancellationToken cancellationToken)
    {
        var project = await repository.GetProjectByExternalIdAsync(request.ExternalId, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, ("ExternalId", "Proyecto no encontrado"));
        }

        // Check user access if requested
        bool? hasAccess = null;
        if (!string.IsNullOrEmpty(request.CheckAccessForUserId))
        {
            hasAccess = await repository.IsUserProjectParticipantAsync(
                project.Id,
                request.CheckAccessForUserId,
                cancellationToken);
        }

        var dto = new ProjectByExternalIdDto(
            project.Id,
            project.ExternalId,
            project.Name,
            project.Description,
            project.BusinessIncubatorId,
            !project.IsDeleted,
            hasAccess);

        return Success(dto);
    }
}