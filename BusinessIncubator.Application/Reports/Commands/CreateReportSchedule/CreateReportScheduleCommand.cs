using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reports.Commands.CreateReportSchedule;

/// <summary>
/// Command to create a report schedule for automated report generation.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record CreateReportScheduleCommand(
    long TemplateId,
    string CronExpression,
    List<string> Recipients,
    string CreatedBy) : IBaseRequest<ReportScheduleResultDto>;

/// <summary>
/// Result DTO for report schedule creation.
/// </summary>
public class ReportScheduleResultDto
{
    /// <summary>
    /// Gets or sets the schedule ID.
    /// </summary>
    public long ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the template ID.
    /// </summary>
    public long TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the CRON expression.
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient count.
    /// </summary>
    public int RecipientCount { get; set; }

    /// <summary>
    /// Gets or sets the next run time.
    /// </summary>
    public DateTime NextRunAt { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Handler for CreateReportScheduleCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateReportScheduleCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="logger">The logger.</param>
public class CreateReportScheduleCommandHandler(
    IReportsRepository repository,
    ILogger<CreateReportScheduleCommandHandler> logger) : BaseCommandHandler<CreateReportScheduleCommand, ReportScheduleResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<ReportScheduleResultDto>> Handle(
        CreateReportScheduleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate template exists
            var template = await repository.GetReportTemplateByIdAsync(request.TemplateId, cancellationToken);
            if (template is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(CreateReportScheduleCommand), $"La plantilla de reporte con ID {request.TemplateId} no fue encontrada."));
            }

            // Validate CRON expression format
            if (!IsValidCronExpression(request.CronExpression))
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(CreateReportScheduleCommand), "La expresión CRON proporcionada no es válida."));
            }

            // Validate recipients
            if (request.Recipients is null || request.Recipients.Count == 0)
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(CreateReportScheduleCommand), "Debe proporcionar al menos un destinatario."));
            }

            // Validate email addresses
            var invalidEmails = request.Recipients.Where(email => !IsValidEmail(email)).ToList();
            if (invalidEmails.Count > 0)
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(CreateReportScheduleCommand), $"Las siguientes direcciones de email no son válidas: {string.Join(", ", invalidEmails)}"));
            }

            // Create schedule using the domain method
            var schedule = template.AddSchedule(
                request.CronExpression,
                request.Recipients,
                request.CreatedBy);

            // Update the template with the new schedule
            repository.UpdateReportTemplate(template);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Report schedule created successfully for template {TemplateId} by user {UserId}. Schedule ID: {ScheduleId}, Next run: {NextRun}",
                request.TemplateId,
                request.CreatedBy,
                schedule.Id,
                schedule.NextRunAt);

            return Success(new ReportScheduleResultDto
            {
                ScheduleId = schedule.Id,
                ExternalId = schedule.ExternalId,
                TemplateId = request.TemplateId,
                CronExpression = request.CronExpression,
                RecipientCount = request.Recipients.Count,
                NextRunAt = schedule.NextRunAt,
                CreatedAt = schedule.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid arguments for creating report schedule for template {TemplateId}", request.TemplateId);
            return Failure(
                ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                (nameof(CreateReportScheduleCommand), ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating report schedule for template {TemplateId}", request.TemplateId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(CreateReportScheduleCommand), "Ocurrió un error al crear la programación del reporte."));
        }
    }

    private static bool IsValidCronExpression(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return false;
        }

        // Basic CRON validation - should have 5 parts (minute hour day month dayOfWeek)
        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
        {
            return false;
        }

        // Validate each part contains valid characters
        var validChars = "0123456789,-*/?";
        return parts.All(part => part.All(c => validChars.Contains(c)));
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
