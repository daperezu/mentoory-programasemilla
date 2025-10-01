using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reports.Queries.GetReportTemplates;

/// <summary>
/// Query to get available report templates for a project or global templates.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetReportTemplatesQuery(
    long? ProjectId,
    ReportType? FilterByType,
    bool IncludeGlobal = true,
    string RequestedBy = "") : IBaseRequest<ReportTemplatesResultDto>;

/// <summary>
/// Result DTO for report templates.
/// </summary>
public class ReportTemplatesResultDto
{
    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the templates.
    /// </summary>
    public List<ReportTemplateDto> Templates { get; set; } = [];
}

/// <summary>
/// DTO for report template.
/// </summary>
public class ReportTemplateDto
{
    /// <summary>
    /// Gets or sets the template ID.
    /// </summary>
    public long TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the report type.
    /// </summary>
    public ReportType Type { get; set; }

    /// <summary>
    /// Gets or sets the type description.
    /// </summary>
    public string TypeDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether it's global.
    /// </summary>
    public bool IsGlobal { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the creator.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether user can edit this template.
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether user can delete this template.
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Gets or sets the number of active schedules.
    /// </summary>
    public int ActiveScheduleCount { get; set; }

    /// <summary>
    /// Gets or sets the configuration preview.
    /// </summary>
    public string ConfigurationPreview { get; set; } = string.Empty;
}

/// <summary>
/// Handler for GetReportTemplatesQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetReportTemplatesQueryHandler"/> class.
/// </remarks>
/// <param name="reportsRepository">The reports repository.</param>
/// <param name="businessIncubatorRepository">The business incubator repository.</param>
/// <param name="logger">The logger.</param>
public class GetReportTemplatesQueryHandler(
    IReportsRepository reportsRepository,
    IBusinessIncubatorRepository businessIncubatorRepository,
    ILogger<GetReportTemplatesQueryHandler> logger) : BaseCommandHandler<GetReportTemplatesQuery, ReportTemplatesResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<ReportTemplatesResultDto>> Handle(
        GetReportTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            List<LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ReportTemplate> templates;

            // Get templates based on request parameters
            if (request.ProjectId.HasValue)
            {
                // Get project-specific and global templates
                templates = await reportsRepository.GetReportTemplatesForProjectAsync(
                    request.ProjectId.Value,
                    cancellationToken);
            }
            else if (request.IncludeGlobal)
            {
                // Get only global templates
                templates = await reportsRepository.GetGlobalReportTemplatesAsync(cancellationToken);
            }
            else
            {
                templates = [];
            }

            // Filter by type if specified
            if (request.FilterByType.HasValue)
            {
                templates = templates.Where(t => t.Type == request.FilterByType.Value).ToList();
            }

            // Map to DTOs
            var templateDtos = new List<ReportTemplateDto>();

            foreach (var template in templates)
            {
                string? projectName = null;
                if (template.ProjectId.HasValue)
                {
                    var project = await businessIncubatorRepository.GetProjectByIdAsync(template.ProjectId.Value, cancellationToken);
                    projectName = project?.Name;
                }

                var dto = new ReportTemplateDto
                {
                    TemplateId = template.Id,
                    ExternalId = template.ExternalId,
                    Name = template.Name,
                    Description = template.Description,
                    Type = template.Type,
                    TypeDescription = GetReportTypeDescription(template.Type),
                    IsGlobal = template.IsGlobal,
                    ProjectId = template.ProjectId,
                    ProjectName = projectName,
                    CreatedAt = template.CreatedAt,
                    CreatedBy = template.CreatedBy,
                    UpdatedAt = template.UpdatedAt,
                    CanEdit = CanEditTemplate(template, request.RequestedBy),
                    CanDelete = CanDeleteTemplate(template, request.RequestedBy),
                    ActiveScheduleCount = template.Schedules.Count(s => s.IsActive),
                    ConfigurationPreview = GetConfigurationPreview(template.ConfigurationJson)
                };

                templateDtos.Add(dto);
            }

            // Sort templates: Global first, then by type, then by name
            templateDtos = templateDtos
                .OrderByDescending(t => t.IsGlobal)
                .ThenBy(t => t.Type)
                .ThenBy(t => t.Name)
                .ToList();

            logger.LogInformation(
                "Retrieved {Count} report templates for project {ProjectId}",
                templateDtos.Count,
                request.ProjectId);

            return Success(new ReportTemplatesResultDto
            {
                TotalCount = templateDtos.Count,
                Templates = templateDtos
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving report templates for project {ProjectId}", request.ProjectId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetReportTemplatesQuery), "Ocurrió un error al obtener las plantillas de reporte."));
        }
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

    private static bool CanEditTemplate(LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ReportTemplate template, string userId)
    {
        // Global templates can only be edited by system administrators
        if (template.IsGlobal)
        {
            return false; // For now, assume coordinators cannot edit global templates
        }

        // Project-specific templates can be edited by the creator or coordinators
        return !string.IsNullOrEmpty(userId);
    }

    private static bool CanDeleteTemplate(LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ReportTemplate template, string userId)
    {
        // Use the domain method to check if template can be deleted
        return template.CanDelete() && !string.IsNullOrEmpty(userId);
    }

    private static string GetConfigurationPreview(string configurationJson)
    {
        // Create a short preview of the configuration
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return "Sin configuración";
        }

        // Truncate if too long
        const int maxLength = 100;
        if (configurationJson.Length <= maxLength)
        {
            return configurationJson;
        }

        return configurationJson[..maxLength] + "...";
    }
}
