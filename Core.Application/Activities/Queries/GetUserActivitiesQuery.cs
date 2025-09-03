using LinaSys.Core.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Activities.Queries;

/// <summary>
/// Query to get recent user activities for dashboard timeline.
/// </summary>
public record GetUserActivitiesQuery(string UserId, int Count = 20) : IBaseRequest<List<UserActivityDto>>;

/// <summary>
/// DTO for user activity information.
/// </summary>
public class UserActivityDto
{
    /// <summary>
    /// Gets or sets the activity identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the activity description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type related to this activity.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the entity ID related to this activity.
    /// </summary>
    public long? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the activity timestamp.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Handler for GetUserActivitiesQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetUserActivitiesQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The user activity repository.</param>
public class GetUserActivitiesQueryHandler(IUserActivityRepository repository) : BaseCommandHandler<GetUserActivitiesQuery, List<UserActivityDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<UserActivityDto>>> Handle(
        GetUserActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        // Get recent activities for the user
        var activities = await repository.GetRecentActivitiesAsync(request.UserId, request.Count);

        var dtos = activities.Select(activity => new UserActivityDto
        {
            Id = activity.Id,
            ActivityType = activity.ActivityType,
            Description = activity.Description,
            EntityType = activity.EntityType,
            EntityId = activity.EntityId,
            CreatedDate = activity.CreatedDate,
            Metadata = activity.Metadata
        }).ToList();

        return Success(dtos);
    }
}