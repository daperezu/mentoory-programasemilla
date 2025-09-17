namespace LinaSys.BusinessIncubator.Application.Public.Queries;

/// <summary>
/// DTO for latest projects response, used when location is not available.
/// </summary>
public class LatestProjectsDto
{
    public List<LatestProjectDto> Projects { get; set; } = new();
    public int TotalFound { get; set; }
    public DateTime SearchedAt { get; set; }
    public string SortedBy { get; set; } = "Fecha de inicio";
}

/// <summary>
/// Individual project DTO with stage information for time-based discovery.
/// </summary>
public class LatestProjectDto
{
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HeroImageBlobId { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? LocationName { get; set; }
    public string? LocationAddress { get; set; }
    public string BusinessIncubatorName { get; set; } = string.Empty;
    public int ActiveParticipants { get; set; }
    public DateTime? LastActivityDate { get; set; }

    // Stage information for sorting and display
    public DateTime? NextStageStartDate { get; set; }
    public string? NextStageTitle { get; set; }
    public string? CurrentPhase { get; set; }
    public List<ProjectStageDto> UpcomingStages { get; set; } = new();

    // Optional location data (null when user hasn't shared location)
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public double? DistanceKm { get; set; }
}

/// <summary>
/// Project stage information for timeline display.
/// </summary>
public class ProjectStageDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsUpcoming { get; set; }
}