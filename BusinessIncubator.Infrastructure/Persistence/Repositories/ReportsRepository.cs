using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.BusinessIncubator.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Reports aggregate operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReportsRepository"/> class.
/// </remarks>
/// <param name="dbContext">The database context.</param>
public class ReportsRepository(BusinessIncubatorDbContext dbContext) : AbstractRepository<ReportTemplate>(dbContext), IReportsRepository
{

    /// <inheritdoc/>
    public async Task AddReportTemplateAsync(ReportTemplate template, CancellationToken cancellationToken = default)
    {
        await dbContext.ReportTemplates.AddAsync(template, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ReportTemplate?> GetReportTemplateByIdAsync(long templateId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReportTemplates
            .Include(t => t.Schedules)
            .FirstOrDefaultAsync(t => t.Id == templateId && !t.IsDeleted, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ReportTemplate?> GetReportTemplateByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReportTemplates
            .Include(t => t.Schedules)
            .FirstOrDefaultAsync(t => t.ExternalId == externalId && !t.IsDeleted, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<ReportTemplate>> GetReportTemplatesForProjectAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReportTemplates
            .Include(t => t.Schedules)
            .Where(t => !t.IsDeleted && (t.IsGlobal || t.ProjectId == projectId))
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<ReportTemplate>> GetGlobalReportTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ReportTemplates
            .Include(t => t.Schedules)
            .Where(t => !t.IsDeleted && t.IsGlobal)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public void UpdateReportTemplate(ReportTemplate template)
    {
        dbContext.ReportTemplates.Update(template);
    }

    /// <inheritdoc/>
    public async Task<List<ReportTemplate>> GetReportTemplatesByTypeAsync(long projectId, ReportType reportType, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReportTemplates
            .Include(t => t.Schedules)
            .Where(t => !t.IsDeleted &&
                       t.Type == reportType &&
                       (t.IsGlobal || t.ProjectId == projectId))
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ReportSchedule?> GetReportScheduleByIdAsync(long scheduleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReportSchedules
            .Include(s => s.Template)
            .FirstOrDefaultAsync(s => s.Id == scheduleId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<ReportSchedule>> GetActiveSchedulesForExecutionAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReportSchedules
            .Include(s => s.Template)
            .Where(s => s.IsActive && s.NextRunAt <= currentTime)
            .OrderBy(s => s.NextRunAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public void UpdateReportSchedule(ReportSchedule schedule)
    {
        dbContext.ReportSchedules.Update(schedule);
    }
}