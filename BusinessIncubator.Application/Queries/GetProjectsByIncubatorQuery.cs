using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get all projects in an incubator.
/// </summary>
public record GetProjectsByIncubatorQuery(long IncubatorId) : IBaseRequest<List<ProjectDto>>;

/// <summary>
/// Handler for GetProjectsByIncubatorQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetProjectsByIncubatorQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetProjectsByIncubatorQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetProjectsByIncubatorQuery, List<ProjectDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<ProjectDto>>> Handle(
        GetProjectsByIncubatorQuery request,
        CancellationToken cancellationToken)
    {
        // Get all projects for the incubator
        var projects = await repository.GetProjectsByIncubatorIdAsync(request.IncubatorId, cancellationToken);

        var dtos = projects
            .Where(p => !p.IsDeleted)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                ExternalId = p.ExternalId,
                Name = p.Name,
                Key = p.Key,
                IsDeleted = p.IsDeleted
            })
            .ToList();

        return Success(dtos);
    }
}
