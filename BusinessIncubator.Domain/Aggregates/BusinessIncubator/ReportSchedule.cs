using System.Text.Json;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Represents a scheduled report generation.
/// </summary>
public class ReportSchedule : Entity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportSchedule"/> class.
    /// </summary>
    /// <param name="templateId">The report template ID.</param>
    /// <param name="cronExpression">The CRON expression for scheduling.</param>
    /// <param name="recipients">The list of recipient email addresses.</param>
    /// <param name="createdBy">The user creating the schedule.</param>
    public ReportSchedule(
        long templateId,
        string cronExpression,
        List<string> recipients,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentException("CRON expression is required.", nameof(cronExpression));
        }

        if (recipients == null || recipients.Count == 0)
        {
            throw new ArgumentException("At least one recipient is required.", nameof(recipients));
        }

        ExternalId = Guid.NewGuid();
        TemplateId = templateId;
        CronExpression = cronExpression;
        Recipients = JsonSerializer.Serialize(recipients);
        IsActive = true;
        NextRunAt = CalculateNextRun(cronExpression);
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportSchedule"/> class.
    /// </summary>
    protected ReportSchedule()
    {
    }

    /// <summary>
    /// Gets the external ID.
    /// </summary>
    public Guid ExternalId { get; private set; }

    /// <summary>
    /// Gets the template ID.
    /// </summary>
    public long TemplateId { get; private set; }

    /// <summary>
    /// Gets the CRON expression.
    /// </summary>
    public string CronExpression { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the recipients JSON array.
    /// </summary>
    public string Recipients { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the schedule is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the last run date.
    /// </summary>
    public DateTime? LastRunAt { get; private set; }

    /// <summary>
    /// Gets the next run date.
    /// </summary>
    public DateTime NextRunAt { get; private set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the creator user ID.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the updater user ID.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the deletion date.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the deleter user ID.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Gets the navigation property to the template.
    /// </summary>
    public virtual ReportTemplate? Template { get; private set; }

    /// <summary>
    /// Gets the recipient email addresses as a list.
    /// </summary>
    /// <returns>List of recipient email addresses.</returns>
    public List<string> GetRecipients()
    {
        if (string.IsNullOrWhiteSpace(Recipients))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(Recipients) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Updates the schedule configuration.
    /// </summary>
    /// <param name="cronExpression">The new CRON expression.</param>
    /// <param name="recipients">The new list of recipients.</param>
    /// <param name="updatedBy">The user updating the schedule.</param>
    public void Update(string cronExpression, List<string> recipients, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentException("CRON expression is required.", nameof(cronExpression));
        }

        if (recipients == null || recipients.Count == 0)
        {
            throw new ArgumentException("At least one recipient is required.", nameof(recipients));
        }

        CronExpression = cronExpression;
        Recipients = JsonSerializer.Serialize(recipients);
        NextRunAt = CalculateNextRun(cronExpression);
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the schedule.
    /// </summary>
    /// <param name="updatedBy">The user activating the schedule.</param>
    public void Activate(string updatedBy)
    {
        IsActive = true;
        NextRunAt = CalculateNextRun(CronExpression);
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the schedule.
    /// </summary>
    /// <param name="updatedBy">The user deactivating the schedule.</param>
    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the schedule as executed and calculates the next run time.
    /// </summary>
    /// <param name="executedAt">The execution timestamp.</param>
    public void MarkAsExecuted(DateTime executedAt)
    {
        LastRunAt = executedAt;
        NextRunAt = CalculateNextRun(CronExpression);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the next run time based on the CRON expression.
    /// </summary>
    /// <param name="cronExpression">The CRON expression.</param>
    /// <returns>The next run date and time.</returns>
    private static DateTime CalculateNextRun(string cronExpression)
    {
        // Simple implementation - in production, use a proper CRON parser like NCrontab or Quartz.NET
        // For now, return a basic calculation based on common patterns

        // Daily at specific hour (e.g., "0 9 * * *" for 9 AM daily)
        if (cronExpression.StartsWith("0 ") && cronExpression.EndsWith(" * * *"))
        {
            if (int.TryParse(cronExpression.Split(' ')[1], out int hour))
            {
                var nextRun = DateTime.Today.AddHours(hour);
                if (nextRun <= DateTime.Now)
                {
                    nextRun = nextRun.AddDays(1);
                }

                return nextRun;
            }
        }

        // Weekly on specific day (e.g., "0 9 * * 1" for Monday 9 AM)
        if (cronExpression.StartsWith("0 ") && cronExpression.Contains(" * * ") && !cronExpression.EndsWith("*"))
        {
            return DateTime.Now.AddDays(7); // Default to weekly
        }

        // Default: daily at current time + 1 day
        return DateTime.Now.AddDays(1);
    }
}