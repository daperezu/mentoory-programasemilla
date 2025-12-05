using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Public.Commands;

/// <summary>
/// Command to record user interest in a project.
/// </summary>
public record RecordProjectInterestCommand(
    Guid ProjectExternalId,
    string? UserId,
    string? SessionId,
    string InterestType,
    decimal? ObserverLatitude,
    decimal? ObserverLongitude,
    string? UserAgent,
    string? IpAddress,
    string? ReferrerUrl) : IBaseRequest<RecordProjectInterestDto>;

/// <summary>
/// DTO for project interest recording result.
/// </summary>
public class RecordProjectInterestDto
{
    public long InterestId { get; set; }
    public Guid ProjectId { get; set; }
    public string InterestType { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public double? DistanceKm { get; set; }
    public bool IsNewInterest { get; set; }
    public string Message { get; set; } = string.Empty;
}
