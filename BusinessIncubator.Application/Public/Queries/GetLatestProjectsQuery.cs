using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Public.Queries;

/// <summary>
/// Query to get the latest projects sorted by start date without requiring location.
/// Used for the default homepage view when users haven't shared their location.
/// </summary>
public record GetLatestProjectsQuery(
    int MaxResults = 10,
    bool IncludeStages = true,
    bool OnlyWithUpcomingStages = true) : IBaseRequest<LatestProjectsDto>;