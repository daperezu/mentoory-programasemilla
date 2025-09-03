using LinaSys.Auth.Application.Queries.Context;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using MediatR;

namespace LinaSys.Orchestration.Application.Context;

/// <summary>
/// Orchestration query to get user's projects with enriched data from BusinessIncubator domain.
/// </summary>
public record GetEnrichedUserProjectsQuery(string UserId, string Role, long IncubatorId) : IBaseRequest<List<EnrichedProjectDto>>;

/// <summary>
/// DTO for enriched project information.
/// </summary>
public class EnrichedProjectDto
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string? UserRole { get; set; }
}

/// <summary>
/// Handler for GetEnrichedUserProjectsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetEnrichedUserProjectsQueryHandler"/> class.
/// </remarks>
/// <param name="mediator">The mediator.</param>
public class GetEnrichedUserProjectsQueryHandler(
    IMediator mediator) : BaseCommandHandler<GetEnrichedUserProjectsQuery, List<EnrichedProjectDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<EnrichedProjectDto>>> Handle(
        GetEnrichedUserProjectsQuery request,
        CancellationToken cancellationToken)
    {
        // Step 1: Get user's project access from Auth domain
        var authQuery = new GetUserProjectsQuery(request.UserId, request.Role, request.IncubatorId);
        var authResult = await mediator.Send(authQuery, cancellationToken);

        if (!authResult.IsSuccess)
        {
            return Failure(ResultErrorCodes.GenericError, ("GetEnrichedUserProjects", "Error al obtener acceso a proyectos"));
        }

        var projectAccess = authResult.Value ?? [];

        if (!projectAccess.Any())
        {
            return Success([]);
        }

        // Check for Global Administrator special marker (-1 ProjectId)
        if (projectAccess is [{ ProjectId: -1, UserRole: Roles.GlobalAdministrator }])
        {
            // Global Administrator - get all projects in the incubator
            var allProjectsQuery = new GetProjectsByIncubatorQuery(request.IncubatorId);
            var allProjectsResult = await mediator.Send(allProjectsQuery, cancellationToken);

            if (!allProjectsResult.IsSuccess)
            {
                return Failure(ResultErrorCodes.GenericError, ("GetEnrichedUserProjects", "Error al obtener proyectos de la incubadora"));
            }

            var globalAdminProjects = allProjectsResult.Value?.Select(p => new EnrichedProjectDto
            {
                ProjectId = p.Id,
                Name = p.Name,
                UserRole = Roles.GlobalAdministrator
            }).ToList() ?? [];

            return Success(globalAdminProjects);
        }

        // Step 2: Get all projects in a single batch query (for non-Global Admin users)
        var projectIds = projectAccess.Select(a => a.ProjectId).ToList();
        var projectsQuery = new GetProjectsByIdsQuery(projectIds);
        var projectsResult = await mediator.Send(projectsQuery, cancellationToken);

        if (!projectsResult.IsSuccess)
        {
            return Failure(ResultErrorCodes.GenericError, ("GetEnrichedUserProjects", "Error al obtener información de proyectos"));
        }

        // Step 3: Combine the access information with project details
        var projectsDict = projectsResult.Value?.ToDictionary(p => p.Id) ?? [];
        var enrichedProjects = new List<EnrichedProjectDto>();

        foreach (var access in projectAccess)
        {
            if (projectsDict.TryGetValue(access.ProjectId, out var project))
            {
                enrichedProjects.Add(new EnrichedProjectDto
                {
                    ProjectId = project.Id,
                    Name = project.Name,
                    UserRole = access.UserRole
                });
            }
        }

        return Success(enrichedProjects);
    }
}
