using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.BusinessIncubator.Domain.Repositories;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get multiple projects by their IDs.
/// </summary>
public record GetProjectsByIdsQuery(List<long> ProjectIds) : IBaseRequest<List<ProjectDto>>;

/// <summary>
/// Handler for GetProjectsByIdsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetProjectsByIdsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetProjectsByIdsQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetProjectsByIdsQuery, List<ProjectDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<ProjectDto>>> Handle(
        GetProjectsByIdsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all projects in a single optimized query
        var projects = await repository.GetProjectsByIdsAsync(request.ProjectIds, cancellationToken);

        // Map to DTOs
        var projectDtos = projects.Select(project => new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Key = project.Key,
            IsDeleted = project.IsDeleted
        }).ToList();

        return Success(projectDtos);
    }
}
