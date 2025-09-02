using LinaSys.Auth.Application.Interfaces;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Auth.Application.Queries.Context;

/// <summary>
/// Query to get user's accessible projects within an incubator.
/// </summary>
public record GetUserProjectsQuery(string UserId, string Role, long IncubatorId) : IBaseRequest<List<UserProjectDto>>;

/// <summary>
/// DTO for user project information.
/// </summary>
public class UserProjectDto
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the project role for the user.
    /// </summary>
    public string? UserRole { get; set; }
}

/// <summary>
/// Handler for GetUserProjectsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetUserProjectsQueryHandler"/> class.
/// </remarks>
/// <param name="projectAccessService">The project access service.</param>
public class GetUserProjectsQueryHandler(IProjectAccessService projectAccessService) : BaseCommandHandler<GetUserProjectsQuery, List<UserProjectDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<UserProjectDto>>> Handle(
        GetUserProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var projects = await projectAccessService.GetUserProjectsAsync(
            request.UserId,
            request.Role,
            request.IncubatorId,
            cancellationToken);

        var result = projects.Select(p => new UserProjectDto
        {
            ProjectId = p.ProjectId,
            Name = p.Name,
            Description = p.Description,
            UserRole = p.UserRole
        }).ToList();

        return Success(result);
    }
}
