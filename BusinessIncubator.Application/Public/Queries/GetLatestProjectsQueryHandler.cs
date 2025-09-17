using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Public.Queries;

/// <summary>
/// Handler for getting latest projects sorted by start date.
/// Primary sorting is by nearest upcoming stage start date, then by project name.
/// </summary>
public class GetLatestProjectsQueryHandler : BaseCommandHandler<GetLatestProjectsQuery, LatestProjectsDto>
{
    private readonly IBusinessIncubatorRepository _repository;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<GetLatestProjectsQueryHandler> _logger;

    public GetLatestProjectsQueryHandler(
        IBusinessIncubatorRepository repository,
        ITimeProvider timeProvider,
        ILogger<GetLatestProjectsQueryHandler> logger)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public override async Task<Result<LatestProjectsDto>> Handle(
        GetLatestProjectsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentDate = _timeProvider.UtcNow;

            // Get all active projects with stages
            var projects = await _repository.GetActiveProjectsWithStagesAsync(cancellationToken);

            // Create projection with calculated fields
            var projectData = new List<ProjectWithUpcomingStages>();

            foreach (var project in projects)
            {
                var upcomingStages = project.ProjectStages
                    .Where(s => s.IsActive && s.EndDate > currentDate)
                    .OrderBy(s => s.StartDate)
                    .ToList();

                // Skip projects without upcoming stages if requested
                if (request.OnlyWithUpcomingStages && !upcomingStages.Any())
                {
                    continue;
                }

                // Count active participants from ProjectUsers collection
                var activeParticipants = project.ProjectUsers?.Count(u => u.IsActive) ?? 0;

                projectData.Add(new ProjectWithUpcomingStages
                {
                    Project = project,
                    UpcomingStages = upcomingStages,
                    ActiveParticipants = activeParticipants,
                    NextStageStartDate = upcomingStages.FirstOrDefault()?.StartDate,
                });
            }

            // Sort by nearest upcoming stage start date, then by name
            var sortedProjects = projectData
                .OrderBy(p => p.NextStageStartDate ?? DateTime.MaxValue)
                .ThenBy(p => p.Project.Name)
                .Take(request.MaxResults)
                .ToList();

            // Get business incubator information
            var incubatorIds = sortedProjects.Select(p => p.Project.BusinessIncubatorId).Distinct().ToList();
            var incubators = await _repository.GetByIdsAsync(incubatorIds, cancellationToken);
            var incubatorDict = incubators.ToDictionary(i => i.Id, i => i.Name);

            // Map to DTOs
            var projectDtos = new List<LatestProjectDto>();
            foreach (var item in sortedProjects)
            {
                var nextStage = item.UpcomingStages.FirstOrDefault();
                var currentStage = item.UpcomingStages.FirstOrDefault(s => s.IsWithinPeriod(currentDate));

                var dto = new LatestProjectDto
                {
                    ExternalId = item.Project.ExternalId,
                    Name = item.Project.Name,
                    Description = item.Project.Description,
                    HeroImageBlobId = item.Project.HeroImageBlobId,
                    LocationName = item.Project.LocationName,
                    LocationAddress = item.Project.LocationAddress,
                    BusinessIncubatorName = incubatorDict.GetValueOrDefault(item.Project.BusinessIncubatorId) ?? "Incubadora",
                    ActiveParticipants = item.ActiveParticipants,
                    LastActivityDate = item.Project.UpdatedAt,
                    NextStageStartDate = nextStage?.StartDate,
                    NextStageTitle = nextStage?.Title,
                    CurrentPhase = currentStage?.Title ?? "Próximamente",
                    Latitude = item.Project.Latitude,
                    Longitude = item.Project.Longitude,
                };

                // Add stage information if requested
                if (request.IncludeStages)
                {
                    dto.UpcomingStages = item.UpcomingStages.Select(s => new ProjectStageDto
                    {
                        Type = s.Type.ToString(),
                        Title = s.Title,
                        Description = s.Description,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        IsActive = s.IsActive,
                        IsCurrent = s.IsWithinPeriod(currentDate),
                        IsUpcoming = s.StartDate > currentDate,
                    }).ToList();
                }

                // Populate image URL
                var encodedBlobId = !string.IsNullOrWhiteSpace(dto.HeroImageBlobId)
                    ? Uri.EscapeDataString(dto.HeroImageBlobId)
                    : "placeholder";

                dto.HeroImageUrl = $"/Public/Images/{encodedBlobId}?type=hero&text={Uri.EscapeDataString(dto.Name ?? "Proyecto")}";

                projectDtos.Add(dto);
            }

            return Success(new LatestProjectsDto
            {
                Projects = projectDtos,
                TotalFound = projectDtos.Count,
                SearchedAt = currentDate,
                SortedBy = "Fecha de inicio",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest projects");
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetLatestProjectsQuery), "Error al obtener los proyectos más recientes."));
        }
    }

    private class ProjectWithUpcomingStages
    {
        public Domain.Aggregates.BusinessIncubator.Project Project { get; set; } = null!;
        public List<Domain.Aggregates.BusinessIncubator.ProjectStage> UpcomingStages { get; set; } = new();
        public int ActiveParticipants { get; set; }
        public DateTime? NextStageStartDate { get; set; }
    }
}