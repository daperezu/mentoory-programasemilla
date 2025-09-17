using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.BusinessIncubator.Application.Project.Queries.GetProjectWithParticipants;

/// <summary>
/// Query to get a project with its participants.
/// </summary>
public sealed record GetProjectWithParticipantsQuery(long ProjectId) : IBaseRequest<ProjectWithParticipantsDto>;

/// <summary>
/// DTO containing project information with participants.
/// </summary>
public class ProjectWithParticipantsDto
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project external identifier.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the list of project users.
    /// </summary>
    public List<ProjectUserDto> ProjectUsers { get; set; } = [];

    /// <summary>
    /// Gets the total number of participants (Starter role only).
    /// </summary>
    public int TotalParticipants => ProjectUsers.Count(u => u.Role == Roles.Starter);

    /// <summary>
    /// Gets the number of active participants (Starter role only).
    /// </summary>
    public int ActiveParticipants => ProjectUsers.Count(u => u.Role == Roles.Starter && u.IsActive);

    /// <summary>
    /// Gets the participants with Starter role only.
    /// </summary>
    public IEnumerable<ProjectUserDto> StarterUsers => ProjectUsers.Where(u => u.Role == Roles.Starter);
}

/// <summary>
/// DTO for project user information.
/// </summary>
public class ProjectUserDto
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when the user joined the project.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets when the user record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Handler for GetProjectWithParticipantsQuery.
/// </summary>
public class GetProjectWithParticipantsQueryHandler(IBusinessIncubatorRepository repository)
    : BaseCommandHandler<GetProjectWithParticipantsQuery, ProjectWithParticipantsDto>
{
    /// <inheritdoc/>
    public override async Task<Result<ProjectWithParticipantsDto>> Handle(
        GetProjectWithParticipantsQuery request,
        CancellationToken cancellationToken)
    {
        var project = await repository.GetProjectWithUsersAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Failure(
                ResultErrorCodes.Project_NotFound,
                ("Project", "El proyecto no existe."));
        }

        var dto = new ProjectWithParticipantsDto
        {
            Id = project.Id,
            Name = project.Name,
            Key = project.Key,
            ExternalId = project.ExternalId,
            ProjectUsers = project.ProjectUsers?.Select(u => new ProjectUserDto
            {
                UserId = u.UserId,
                Role = u.Role,
                IsActive = u.IsActive,
                JoinedAt = u.JoinedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList() ?? []
        };

        return Success(dto);
    }
}