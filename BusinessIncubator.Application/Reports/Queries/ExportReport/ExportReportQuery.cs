using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reports.Queries.ExportReport;

/// <summary>
/// Export format enumeration.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Excel format (.xlsx).
    /// </summary>
    Excel = 0,

    /// <summary>
    /// CSV format (.csv).
    /// </summary>
    CSV = 1
}

/// <summary>
/// Query to export a report in the specified format.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record ExportReportQuery(
    long TemplateId,
    long? ProjectId,
    DateTime? StartDate,
    DateTime? EndDate,
    ExportFormat Format,
    Dictionary<string, object>? Parameters,
    string RequestedBy) : IBaseRequest<ExportReportResultDto>;

/// <summary>
/// Result DTO for report export.
/// </summary>
public class ExportReportResultDto
{
    /// <summary>
    /// Gets or sets the export ID.
    /// </summary>
    public string ExportId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file content as base64.
    /// </summary>
    public string FileContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the export timestamp.
    /// </summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>
    /// Gets or sets the report metadata.
    /// </summary>
    public ExportMetadataDto? Metadata { get; set; }
}

/// <summary>
/// DTO for export metadata.
/// </summary>
public class ExportMetadataDto
{
    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the report type.
    /// </summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the date range.
    /// </summary>
    public string DateRange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total records exported.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the export duration.
    /// </summary>
    public TimeSpan ExportDuration { get; set; }
}

/// <summary>
/// Handler for ExportReportQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ExportReportQueryHandler"/> class.
/// </remarks>
/// <param name="reportsRepository">The reports repository.</param>
/// <param name="businessIncubatorRepository">The business incubator repository.</param>
/// <param name="auditContext">The audit context.</param>
/// <param name="logger">The logger.</param>
public class ExportReportQueryHandler(
    IReportsRepository reportsRepository,
    IBusinessIncubatorRepository businessIncubatorRepository,
    IAuditContext auditContext,
    ILogger<ExportReportQueryHandler> logger) : BaseCommandHandler<ExportReportQuery, ExportReportResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<ExportReportResultDto>> Handle(
        ExportReportQuery request,
        CancellationToken cancellationToken)
    {
        var startTime = auditContext.UtcNow;

        try
        {
            // Get report template
            var template = await reportsRepository.GetReportTemplateByIdAsync(request.TemplateId, cancellationToken);
            if (template is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(ExportReportQuery), $"La plantilla de reporte con ID {request.TemplateId} no fue encontrada."));
            }

            // Validate project access for project-specific templates
            if (!template.IsGlobal && template.ProjectId.HasValue)
            {
                if (request.ProjectId != template.ProjectId)
                {
                    return Failure(
                        ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource,
                        (nameof(ExportReportQuery), "No tiene permisos para exportar reportes de este proyecto."));
                }
            }

            // Set default date range if not provided
            var startDate = request.StartDate ?? auditContext.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? auditContext.UtcNow;

            if (startDate > endDate)
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(ExportReportQuery), "La fecha de inicio no puede ser mayor a la fecha final."));
            }

            // Get project information
            string? projectName = null;
            if (request.ProjectId.HasValue)
            {
                var project = await businessIncubatorRepository.GetProjectByIdAsync(request.ProjectId.Value, cancellationToken);
                projectName = project?.Name;
            }

            // Generate report data based on template type
            var reportData = await GenerateReportDataAsync(template, request.ProjectId, startDate, endDate, cancellationToken);

            // Create export file
            var exportResult = await CreateExportFileAsync(
                template,
                reportData,
                request.Format,
                projectName,
                startDate,
                endDate);

            var exportDuration = auditContext.UtcNow - startTime;

            logger.LogInformation(
                "Report exported successfully. Template: {TemplateId}, Format: {Format}, Records: {RecordCount}, Duration: {Duration}ms",
                request.TemplateId,
                request.Format,
                reportData.Count,
                exportDuration.TotalMilliseconds);

            return Success(new ExportReportResultDto
            {
                ExportId = Guid.NewGuid().ToString(),
                FileName = exportResult.FileName,
                ContentType = exportResult.ContentType,
                FileContent = exportResult.FileContent,
                FileSizeBytes = exportResult.FileSizeBytes,
                ExportedAt = auditContext.UtcNow,
                Metadata = new ExportMetadataDto
                {
                    TemplateName = template.Name,
                    ReportType = GetReportTypeDescription(template.Type),
                    ProjectName = projectName,
                    DateRange = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                    TotalRecords = reportData.Count,
                    ExportDuration = exportDuration
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting report from template {TemplateId}", request.TemplateId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(ExportReportQuery), "Ocurrió un error al exportar el reporte."));
        }
    }

    private async Task<List<Dictionary<string, object>>> GenerateReportDataAsync(
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ReportTemplate template,
        long? projectId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var reportData = new List<Dictionary<string, object>>();

        if (!projectId.HasValue)
        {
            return reportData;
        }

        switch (template.Type)
        {
            case ReportType.Progress:
                reportData = await GenerateProgressReportAsync(projectId.Value, startDate, endDate, cancellationToken);
                break;

            case ReportType.Completion:
                reportData = await GenerateCompletionReportAsync(projectId.Value, startDate, endDate, cancellationToken);
                break;

            case ReportType.Participation:
                reportData = await GenerateParticipationReportAsync(projectId.Value, startDate, endDate, cancellationToken);
                break;

            case ReportType.Custom:
                reportData = await GenerateCustomReportAsync(projectId.Value, template.ConfigurationJson, startDate, endDate, cancellationToken);
                break;
        }

        return reportData;
    }

    private async Task<List<Dictionary<string, object>>> GenerateProgressReportAsync(
        long projectId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var reportData = new List<Dictionary<string, object>>();

        var project = await businessIncubatorRepository.GetProjectWithFormSubmissionsAsync(projectId, cancellationToken);
        if (project is null)
        {
            return reportData;
        }

        var submissions = project.FormSubmissions
            .Where(s => s.StartedAt >= startDate && s.StartedAt <= endDate)
            .OrderBy(s => s.StartedAt);

        foreach (var submission in submissions)
        {
            reportData.Add(new Dictionary<string, object>
            {
                ["Fecha_Inicio"] = submission.StartedAt,
                ["Fecha_Envio"] = submission.SubmittedAt?.ToString() ?? "En progreso",
                ["Usuario"] = submission.ParticipantUserId,
                ["Estado"] = submission.Status.ToString(),
                ["Progreso"] = CalculateProgress(submission),
                ["Proyecto"] = project.Name
            });
        }

        return reportData;
    }

    private async Task<List<Dictionary<string, object>>> GenerateCompletionReportAsync(
        long projectId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var reportData = new List<Dictionary<string, object>>();

        var project = await businessIncubatorRepository.GetProjectWithFormSubmissionsAsync(projectId, cancellationToken);
        if (project is null)
        {
            return reportData;
        }

        var completedSubmissions = project.FormSubmissions
            .Where(s => s.SubmittedAt.HasValue &&
                       s.SubmittedAt >= startDate &&
                       s.SubmittedAt <= endDate)
            .OrderBy(s => s.SubmittedAt);

        foreach (var submission in completedSubmissions)
        {
            reportData.Add(new Dictionary<string, object>
            {
                ["Fecha_Finalizacion"] = submission.SubmittedAt!.Value,
                ["Usuario"] = submission.ParticipantUserId,
                ["Tiempo_Total"] = CalculateTotalTime(submission),
                ["Estado_Final"] = submission.Status.ToString(),
                ["Proyecto"] = project.Name,
                ["Revision_Requerida"] = submission.Status == ProjectFormSubmissionStatus.Submitted ? "Sí" : "No"
            });
        }

        return reportData;
    }

    private async Task<List<Dictionary<string, object>>> GenerateParticipationReportAsync(
        long projectId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var reportData = new List<Dictionary<string, object>>();

        var project = await businessIncubatorRepository.GetProjectWithUsersAsync(projectId, cancellationToken);
        if (project is null)
        {
            return reportData;
        }

        var projectUsers = project.ProjectUsers
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .OrderBy(u => u.CreatedAt);

        foreach (var user in projectUsers)
        {
            reportData.Add(new Dictionary<string, object>
            {
                ["Fecha_Invitacion"] = user.CreatedAt,
                ["Usuario"] = user.UserId,
                ["Rol"] = user.Role.ToString(),
                ["Estado"] = user.IsActive ? "Activo" : "Inactivo",
                ["Proyecto"] = project.Name,
                ["Ultimo_Acceso"] = user.LeftAt?.ToString() ?? "Activo"
            });
        }

        return reportData;
    }

    private async Task<List<Dictionary<string, object>>> GenerateCustomReportAsync(
        long projectId,
        string configurationJson,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        // For custom reports, we would parse the configuration and generate data accordingly
        // For now, return progress data as a fallback
        return await GenerateProgressReportAsync(projectId, startDate, endDate, cancellationToken);
    }

#pragma warning disable SA1204 // Static members should appear before instance members
    private static async Task<ExportFileResult> CreateExportFileAsync(
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ReportTemplate template,
        List<Dictionary<string, object>> reportData,
        ExportFormat format,
        string? projectName,
        DateTime startDate,
        DateTime endDate)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var projectSuffix = !string.IsNullOrEmpty(projectName) ? $"_{projectName}" : string.Empty;
        var fileName = $"{template.Name}{projectSuffix}_{timestamp}";

        switch (format)
        {
            case ExportFormat.Excel:
                return await CreateExcelFileAsync(fileName, reportData);

            case ExportFormat.CSV:
                return CreateCsvFile(fileName, reportData);

            default:
                throw new ArgumentException("Formato de exportación no soportado", nameof(format));
        }
    }

    private static Task<ExportFileResult> CreateExcelFileAsync(string fileName, List<Dictionary<string, object>> reportData)
    {
        // This would use EPPlus similar to ParticipantExcelService
        // For now, return a placeholder implementation
        var content = "Excel content placeholder";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);

        return Task.FromResult(new ExportFileResult
        {
            FileName = $"{fileName}.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileContent = Convert.ToBase64String(bytes),
            FileSizeBytes = bytes.Length
        });
    }

    private static ExportFileResult CreateCsvFile(string fileName, List<Dictionary<string, object>> reportData)
    {
        var csvBuilder = new System.Text.StringBuilder();

        // Add headers
        if (reportData.Count > 0)
        {
            var headers = string.Join(",", reportData[0].Keys);
            csvBuilder.AppendLine(headers);

            // Add data rows
            foreach (var row in reportData)
            {
                var values = string.Join(",", row.Values.Select(v => $"\"{v}\""));
                csvBuilder.AppendLine(values);
            }
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());

        return new ExportFileResult
        {
            FileName = $"{fileName}.csv",
            ContentType = "text/csv",
            FileContent = Convert.ToBase64String(bytes),
            FileSizeBytes = bytes.Length
        };
    }

    private static string CalculateProgress(LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectFormSubmission submission)
    {
        // Calculate progress based on submission status and data
        return submission.Status switch
        {
            ProjectFormSubmissionStatus.Draft => "En progreso",
            ProjectFormSubmissionStatus.Submitted => "100%",
            ProjectFormSubmissionStatus.Approved => "100%",
            ProjectFormSubmissionStatus.Rejected => "100%",
            _ => "0%"
        };
    }

    private static string CalculateTotalTime(LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectFormSubmission submission)
    {
        if (!submission.SubmittedAt.HasValue)
        {
            return "En progreso";
        }

        var duration = submission.SubmittedAt.Value - submission.StartedAt;
        return $"{duration.TotalHours:F1} horas";
    }

    private static string GetReportTypeDescription(ReportType reportType)
    {
        return reportType switch
        {
            ReportType.Progress => "Progreso",
            ReportType.Completion => "Finalización",
            ReportType.Participation => "Participación",
            ReportType.Custom => "Personalizado",
            _ => "Desconocido"
        };
    }
}

/// <summary>
/// Internal class for export file results.
/// </summary>
internal class ExportFileResult
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FileContent { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
