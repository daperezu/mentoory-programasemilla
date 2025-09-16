namespace LinaSys.BusinessIncubator.Application.Public.Queries;

/// <summary>
/// Detailed information about a project for public display.
/// </summary>
public class ProjectDetailDto
{
    /// <summary>
    /// Gets or sets the project's external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the project status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the business incubator name.
    /// </summary>
    public string BusinessIncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the business incubator description.
    /// </summary>
    public string? BusinessIncubatorDescription { get; set; }

    /// <summary>
    /// Gets or sets the business incubator external ID.
    /// </summary>
    public Guid BusinessIncubatorExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project's hero image URL.
    /// </summary>
    public string? HeroImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the project's hero image blob ID.
    /// </summary>
    public string? HeroImageBlobId { get; set; }

    /// <summary>
    /// Gets or sets the geolocation latitude.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the geolocation longitude.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Gets or sets the number of active participants.
    /// </summary>
    public int ActiveParticipants { get; set; }

    /// <summary>
    /// Gets or sets the project start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the project end date.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the current stage name.
    /// </summary>
    public string? CurrentStageName { get; set; }

    /// <summary>
    /// Gets or sets the project stages.
    /// </summary>
    public List<ProjectStageDto> Stages { get; set; } = new();
}