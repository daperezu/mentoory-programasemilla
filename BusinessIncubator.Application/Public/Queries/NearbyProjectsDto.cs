namespace LinaSys.BusinessIncubator.Application.Public.Queries;

public class NearbyProjectsDto
{
    public decimal UserLatitude { get; set; }
    public decimal UserLongitude { get; set; }
    public double SearchRadiusKm { get; set; }
    public List<NearbyProjectDto> Projects { get; set; } = new();
    public int TotalFound { get; set; }
    public DateTime SearchedAt { get; set; }
}

public class NearbyProjectDto
{
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HeroImageBlobId { get; set; }
    public string? HeroImageUrl { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? LocationName { get; set; }
    public string? LocationAddress { get; set; }
    public double DistanceKm { get; set; }
    public string BusinessIncubatorName { get; set; } = string.Empty;
    public int ActiveParticipants { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public string? CurrentPhase { get; set; }
}