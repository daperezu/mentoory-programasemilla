using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Projects.Queries.GetProjectByExternalId;

/// <summary>
/// Query to get a project by its external ID.
/// </summary>
public record GetProjectByExternalIdQuery(Guid ExternalId) : IBaseRequest<ProjectDto>;

/// <summary>
/// DTO for project information.
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// Gets or sets the internal project ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the external project ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the business incubator ID.
    /// </summary>
    public long BusinessIncubatorId { get; set; }
}

/// <summary>
/// Handler for GetProjectByExternalIdQuery.
/// </summary>
public class GetProjectByExternalIdQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetProjectByExternalIdQueryHandler> logger)
    : BaseCommandHandler<GetProjectByExternalIdQuery, ProjectDto>
{
    /// <inheritdoc/>
    public override async Task<Result<ProjectDto>> Handle(
        GetProjectByExternalIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await repository.GetProjectByExternalIdAsync(
                request.ExternalId,
                cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound,
                    ("project", "El proyecto no fue encontrado"));
            }

            var dto = new ProjectDto
            {
                Id = project.Id,
                ExternalId = project.ExternalId,
                Name = project.Name,
                BusinessIncubatorId = project.BusinessIncubatorId
            };

            return Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error retrieving project with external ID {ExternalId}",
                request.ExternalId);

            return Failure(ResultErrorCodes.GenericError,
                ("error", "Error al obtener el proyecto"));
        }
    }
}