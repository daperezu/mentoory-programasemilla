using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

public sealed record GetProjectWithStagesQuery(long ProjectId) : IBaseRequest<ProjectWithStagesDto>;

public sealed record ProjectWithStagesDto(
    long Id,
    string Name,
    string? Description,
    long IncubatorId,
    List<ProjectStageDto> Stages);

public sealed record ProjectStageDto(
    long Id,
    string Name,
    string? Description,
    int Order,
    bool IsActive);

public sealed class GetProjectWithStagesQueryHandler(
    IBusinessIncubatorRepository repository) : BaseCommandHandler<GetProjectWithStagesQuery, ProjectWithStagesDto>
{
    public override async Task<Result<ProjectWithStagesDto>> Handle(
        GetProjectWithStagesQuery request,
        CancellationToken cancellationToken)
    {
        var project = await repository.GetProjectWithStagesAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, ("ProjectId", "Proyecto no encontrado"));
        }

        var dto = new ProjectWithStagesDto(
            project.Id,
            project.Name,
            project.Description,
            project.BusinessIncubatorId,
            project.ProjectStages.Select(s => new ProjectStageDto(
                s.Id,
                s.Title,
                s.Description,
                (int)s.Type,
                s.IsActive)).ToList());

        return Success(dto);
    }
}