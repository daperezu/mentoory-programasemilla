using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorParticipantStats;

/// <summary>
/// Query to get participant statistics for a coordinator's project.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetCoordinatorParticipantStatsQuery(long ProjectId) : IBaseRequest<CoordinatorParticipantStatsDto>;

/// <summary>
/// DTO for coordinator participant statistics.
/// </summary>
public class CoordinatorParticipantStatsDto
{
    /// <summary>
    /// Gets or sets the total number of participants.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets participant count by role.
    /// </summary>
    public List<ParticipantRoleStats> ByRole { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of active participants.
    /// </summary>
    public int ActiveCount { get; set; }

    /// <summary>
    /// Gets or sets the number of pending invitations.
    /// </summary>
    public int PendingInvitations { get; set; }

    /// <summary>
    /// Gets or sets the number of recently added participants (last 7 days).
    /// </summary>
    public int RecentlyAdded { get; set; }
}

/// <summary>
/// Participant statistics by role.
/// </summary>
public class ParticipantRoleStats
{
    /// <summary>
    /// Gets or sets the role name.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the count for this role.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the percentage of total.
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// Handler for GetCoordinatorParticipantStatsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCoordinatorParticipantStatsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetCoordinatorParticipantStatsQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetCoordinatorParticipantStatsQuery, CoordinatorParticipantStatsDto>
{

    /// <inheritdoc/>
    public override async Task<Result<CoordinatorParticipantStatsDto>> Handle(
        GetCoordinatorParticipantStatsQuery request,
        CancellationToken cancellationToken)
    {
        // Get project with users
        var project = await repository.GetProjectWithUsersAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                (nameof(GetCoordinatorParticipantStatsQuery), $"Project with ID {request.ProjectId} not found."));
        }

        // Get actual user counts
        var activeUsers = project.GetActiveUsers().ToList();
        var total = activeUsers.Count;
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        // Get role statistics
        var roleGroups = project.GetUserCountByRole()
            .Select(kvp => new ParticipantRoleStats
            {
                Role = kvp.Key,
                Count = kvp.Value,
                Percentage = total > 0 ? Math.Round((double)kvp.Value / total * 100, 1) : 0,
            })
            .ToList();

        // Count pending invitations
        var projectWithInvitations = await repository.GetProjectWithInvitationsByExternalIdAsync(project.ExternalId, cancellationToken);
        var pendingInvitations = projectWithInvitations?.ProjectInvitations
            .Count(i => i.Status == Domain.Enums.ProjectInvitationStatus.Pending) ?? 0;

        // Count recently added users
        var recentlyAdded = project.GetRecentlyJoinedUsers(sevenDaysAgo).Count();

        var result = new CoordinatorParticipantStatsDto
        {
            Total = total,
            ByRole = roleGroups,
            ActiveCount = project.GetActiveUserCount(),
            PendingInvitations = pendingInvitations,
            RecentlyAdded = recentlyAdded,
        };

        return Success(result);
    }
}
