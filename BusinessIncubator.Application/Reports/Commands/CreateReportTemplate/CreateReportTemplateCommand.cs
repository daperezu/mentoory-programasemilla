using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reports.Commands.CreateReportTemplate;

/// <summary>
/// Command to create a new report template.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record CreateReportTemplateCommand(
    string Name,
    string? Description,
    ReportType Type,
    bool IsGlobal,
    long? ProjectId,
    string ConfigurationJson,
    string CreatedBy) : IBaseRequest<ReportTemplateResultDto>;

/// <summary>
/// Result DTO for report template creation.
/// </summary>
public class ReportTemplateResultDto
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
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Handler for CreateReportTemplateCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateReportTemplateCommandHandler"/> class.
/// </remarks>
/// <param name="reportsRepository">The reports repository.</param>
/// <param name="businessIncubatorRepository">The business incubator repository.</param>
/// <param name="logger">The logger.</param>
public class CreateReportTemplateCommandHandler(
    IReportsRepository reportsRepository,
    IBusinessIncubatorRepository businessIncubatorRepository,
    ILogger<CreateReportTemplateCommandHandler> logger) : BaseCommandHandler<CreateReportTemplateCommand, ReportTemplateResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<ReportTemplateResultDto>> Handle(
        CreateReportTemplateCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate project existence for project-specific templates
            if (!request.IsGlobal && request.ProjectId.HasValue)
            {
                var project = await businessIncubatorRepository.GetProjectByIdAsync(request.ProjectId.Value, cancellationToken);
                if (project is null)
                {
                    return Failure(
                        ResultErrorCodes.BusinessIncubator_NotFound,
                        (nameof(CreateReportTemplateCommand), $"El proyecto con ID {request.ProjectId} no fue encontrado."));
                }
            }

            // Create report template
            var template = new ReportTemplate(
                request.Name,
                request.Description,
                request.Type,
                request.IsGlobal,
                request.ProjectId,
                request.ConfigurationJson,
                request.CreatedBy);

            // Save to repository
            await reportsRepository.AddReportTemplateAsync(template, cancellationToken);
            await reportsRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Report template {TemplateName} created successfully by user {UserId}. Template ID: {TemplateId}",
                request.Name,
                request.CreatedBy,
                template.Id);

            return Success(new ReportTemplateResultDto
            {
                TemplateId = template.Id,
                ExternalId = template.ExternalId,
                Name = template.Name,
                CreatedAt = template.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid arguments for creating report template {TemplateName}", request.Name);
            return Failure(
                ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                (nameof(CreateReportTemplateCommand), ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating report template {TemplateName}", request.Name);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(CreateReportTemplateCommand), "Ocurrió un error al crear la plantilla de reporte."));
        }
    }
}
