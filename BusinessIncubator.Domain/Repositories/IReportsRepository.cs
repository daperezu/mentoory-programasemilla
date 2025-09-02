using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Repositories;

/// <summary>
/// Repository interface for Reports aggregate operations.
/// </summary>
public interface IReportsRepository : IRepository<ReportTemplate>
{
    /// <summary>
    /// Adds a new report template to the repository.
    /// </summary>
    /// <param name="template">The report template to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddReportTemplateAsync(ReportTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a report template by its ID.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The report template if found, otherwise null.</returns>
    Task<ReportTemplate?> GetReportTemplateByIdAsync(long templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a report template by its external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The report template if found, otherwise null.</returns>
    Task<ReportTemplate?> GetReportTemplateByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all report templates for a specific project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of report templates for the project.</returns>
    Task<List<ReportTemplate>> GetReportTemplatesForProjectAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all global report templates.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of global report templates.</returns>
    Task<List<ReportTemplate>> GetGlobalReportTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report template.
    /// </summary>
    /// <param name="template">The report template to update.</param>
    void UpdateReportTemplate(ReportTemplate template);

    /// <summary>
    /// Gets report templates by type for a specific project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="reportType">The report type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of report templates of the specified type.</returns>
    Task<List<ReportTemplate>> GetReportTemplatesByTypeAsync(long projectId, ReportType reportType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a report schedule by its ID.
    /// </summary>
    /// <param name="scheduleId">The schedule ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The report schedule if found, otherwise null.</returns>
    Task<ReportSchedule?> GetReportScheduleByIdAsync(long scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active schedules that are due for execution.
    /// </summary>
    /// <param name="currentTime">The current time to check against.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of active schedules ready for execution.</returns>
    Task<List<ReportSchedule>> GetActiveSchedulesForExecutionAsync(DateTime currentTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report schedule.
    /// </summary>
    /// <param name="schedule">The report schedule to update.</param>
    void UpdateReportSchedule(ReportSchedule schedule);
}