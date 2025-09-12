using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Orchestration.Application.Diagnostics.Queries;

/// <summary>
/// Query to fetch presentation data for diagnosis charts.
/// </summary>
public record GetDiagnosisChartPresentationDataQuery(
    Guid ProjectExternalId,
    string ParticipantUserId) : IBaseRequest<DiagnosisChartPresentationDto>;

/// <summary>
/// DTO for diagnosis chart presentation data.
/// </summary>
public class DiagnosisChartPresentationDto
{
    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant name.
    /// </summary>
    public string ParticipantName { get; set; } = string.Empty;
}