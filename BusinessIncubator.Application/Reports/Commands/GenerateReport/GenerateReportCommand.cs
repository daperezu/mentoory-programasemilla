using System.Text.Json;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reports.Commands.GenerateReport;

/// <summary>
/// Command to generate a report from a template.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GenerateReportCommand(
    long TemplateId,
    long? ProjectId,
    DateTime? StartDate,
    DateTime? EndDate,
    Dictionary<string, object>? Parameters,
    string RequestedBy) : IBaseRequest<GenerateReportResultDto>;

/// <summary>
/// Result DTO for report generation.
/// </summary>
public class GenerateReportResultDto
{
    /// <summary>
    /// Gets or sets the report ID.
    /// </summary>
    public string ReportId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template ID.
    /// </summary>
    public long TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the report status.
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the generation start time.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets the estimated completion time.
    /// </summary>
    public DateTime? EstimatedCompletion { get; set; }

    /// <summary>
    /// Gets or sets the data summary.
    /// </summary>
    public ReportDataSummaryDto? DataSummary { get; set; }
}

/// <summary>
/// DTO for report data summary.
/// </summary>
public class ReportDataSummaryDto
{
    /// <summary>
    /// Gets or sets the total records count.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the date range.
    /// </summary>
    public string DateRange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the report type description.
    /// </summary>
    public string ReportType { get; set; } = string.Empty;
}

/// <summary>
/// Handler for GenerateReportCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GenerateReportCommandHandler"/> class.
/// </remarks>
/// <param name="reportsRepository">The reports repository.</param>
/// <param name="businessIncubatorRepository">The business incubator repository.</param>
/// <param name="logger">The logger.</param>
public class GenerateReportCommandHandler(
    IReportsRepository reportsRepository,
    IBusinessIncubatorRepository businessIncubatorRepository,
    ILogger<GenerateReportCommandHandler> logger) : BaseCommandHandler<GenerateReportCommand, GenerateReportResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<GenerateReportResultDto>> Handle(
        GenerateReportCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get report template
            var template = await reportsRepository.GetReportTemplateByIdAsync(request.TemplateId, cancellationToken);
            if (template is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(GenerateReportCommand), $"La plantilla de reporte con ID {request.TemplateId} no fue encontrada."));
            }

            // Validate project access for project-specific templates
            if (!template.IsGlobal && template.ProjectId.HasValue)
            {
                if (request.ProjectId != template.ProjectId)
                {
                    return Failure(
                        ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource,
                        (nameof(GenerateReportCommand), "No tiene permisos para generar reportes de este proyecto."));
                }
            }

            // Set default date range if not provided
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            if (startDate > endDate)
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(GenerateReportCommand), "La fecha de inicio no puede ser mayor a la fecha final."));
            }

            // Get project data for summary
            string? projectName = null;
            int totalRecords = 0;

            if (request.ProjectId.HasValue)
            {
                var project = await businessIncubatorRepository.GetProjectByIdAsync(request.ProjectId.Value, cancellationToken);
                projectName = project?.Name;

                // Calculate records based on report type
                totalRecords = template.Type switch
                {
                    ReportType.Progress => await GetProgressRecordsCountAsync(request.ProjectId.Value, startDate, endDate, cancellationToken),
                    ReportType.Completion => await GetCompletionRecordsCountAsync(request.ProjectId.Value, startDate, endDate, cancellationToken),
                    ReportType.Participation => await GetParticipationRecordsCountAsync(request.ProjectId.Value, startDate, endDate, cancellationToken),
                    ReportType.Custom => await GetCustomRecordsCountAsync(request.ProjectId.Value, template.ConfigurationJson, startDate, endDate, cancellationToken),
                    _ => 0
                };
            }

            // Generate unique report ID
            var reportId = Guid.NewGuid().ToString();

            // Estimate completion time based on data volume and report complexity
            var estimatedCompletion = EstimateCompletionTime(template.Type, totalRecords);

            logger.LogInformation(
                "Report generation started. Template: {TemplateId}, Project: {ProjectId}, Records: {TotalRecords}, RequestedBy: {RequestedBy}",
                request.TemplateId,
                request.ProjectId,
                totalRecords,
                request.RequestedBy);

            return Success(new GenerateReportResultDto
            {
                ReportId = reportId,
                TemplateId = template.Id,
                Status = ReportStatus.Pending,
                GeneratedAt = DateTime.UtcNow,
                EstimatedCompletion = estimatedCompletion,
                DataSummary = new ReportDataSummaryDto
                {
                    TotalRecords = totalRecords,
                    DateRange = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                    ProjectName = projectName,
                    ReportType = GetReportTypeDescription(template.Type)
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating report from template {TemplateId}", request.TemplateId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GenerateReportCommand), "Ocurrió un error al generar el reporte."));
        }
    }

    private static DateTime EstimateCompletionTime(ReportType reportType, int totalRecords)
    {
        // Estimate processing time based on report type and data volume
        var baseMinutes = reportType switch
        {
            ReportType.Progress => 2,
            ReportType.Completion => 3,
            ReportType.Participation => 1,
            ReportType.Custom => 5,
            _ => 2
        };

        // Add time based on record count
        var additionalMinutes = totalRecords switch
        {
            > 10000 => 15,
            > 5000 => 10,
            > 1000 => 5,
            > 100 => 2,
            _ => 0
        };

        return DateTime.UtcNow.AddMinutes(baseMinutes + additionalMinutes);
    }

    private static string GetReportTypeDescription(ReportType reportType)
    {
        return reportType switch
        {
            ReportType.Progress => "Reporte de Progreso",
            ReportType.Completion => "Reporte de Finalización",
            ReportType.Participation => "Reporte de Participación",
            ReportType.Custom => "Reporte Personalizado",
            _ => "Reporte"
        };
    }

    private async Task<int> GetProgressRecordsCountAsync(long projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        // Get form submissions in the date range
        var project = await businessIncubatorRepository.GetProjectWithFormSubmissionsAsync(projectId, cancellationToken);
        if (project is null)
        {
            return 0;
        }

        return project.FormSubmissions.Count(s =>
            s.StartedAt >= startDate && s.StartedAt <= endDate);
    }

    private async Task<int> GetCompletionRecordsCountAsync(long projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        // Get completed submissions in the date range
        var project = await businessIncubatorRepository.GetProjectWithFormSubmissionsAsync(projectId, cancellationToken);
        if (project is null)
        {
            return 0;
        }

        return project.FormSubmissions.Count(s =>
            s.SubmittedAt.HasValue &&
            s.SubmittedAt >= startDate &&
            s.SubmittedAt <= endDate);
    }

    private async Task<int> GetParticipationRecordsCountAsync(long projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        // Get project users count
        var project = await businessIncubatorRepository.GetProjectWithUsersAsync(projectId, cancellationToken);
        if (project is null)
        {
            return 0;
        }

        return project.ProjectUsers.Count(u =>
            u.CreatedAt >= startDate && u.CreatedAt <= endDate);
    }

    private async Task<int> GetCustomRecordsCountAsync(long projectId, string configurationJson, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        // Parse custom configuration to determine data source
        try
        {
            var config = JsonDocument.Parse(configurationJson);

            // For now, return a basic count based on the most common scenario
            return await GetProgressRecordsCountAsync(projectId, startDate, endDate, cancellationToken);
        }
        catch
        {
            return 0;
        }
    }
}
