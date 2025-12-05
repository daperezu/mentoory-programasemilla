using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.BusinessIncubator.Domain.Repositories;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get a project by its ID.
/// </summary>
public record GetProjectByIdQuery(long ProjectId) : IBaseRequest<ProjectDto?>;

/// <summary>
/// DTO for project information.
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the project external identifier.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the project is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Handler for GetProjectByIdQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetProjectByIdQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetProjectByIdQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetProjectByIdQuery, ProjectDto?>
{

    /// <inheritdoc/>
    public override async Task<Result<ProjectDto?>> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        var project = await repository.GetProjectByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Success(null);
        }

        var dto = new ProjectDto
        {
            Id = project.Id,
            ExternalId = project.ExternalId,
            Name = project.Name,
            Key = project.Key,
            IsDeleted = project.IsDeleted
        };

        return Success(dto);
    }
}
