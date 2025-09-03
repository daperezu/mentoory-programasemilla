using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public class ProjectStage : AuditableEntity
{
    protected ProjectStage()
    {
    }

    /// <summary>
    /// Gets the description of this stage.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the end date of this stage.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Gets whether this stage is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the project ID this stage belongs to.
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// Gets the start date of this stage.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the title of this stage.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the type of this stage.
    /// </summary>
    public ProjectStageType Type { get; private set; }

    /// <summary>
    /// Navigation property for EF Core.
    /// </summary>
    internal virtual Project Project { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectStage"/> class.
    /// Factory method for creating new stages.
    /// </summary>
    public static ProjectStage Create(
        long projectId,
        ProjectStageType type,
        string title,
        string? description,
        DateTime startDate,
        DateTime endDate,
        IAuditContext auditContext)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Project ID must be positive.", nameof(projectId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        if (title.Length > 200)
        {
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));
        }

        if (!string.IsNullOrEmpty(description) && description.Length > 2000)
        {
            throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(description));
        }

        if (startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date.", nameof(startDate));
        }

        var stage = new ProjectStage
        {
            ProjectId = projectId,
            Type = type,
            Title = title.Trim(),
            Description = description?.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsActive = false // Stages start as inactive by default
        };

        stage.SetCreated(auditContext);
        return stage;
    }

    /// <summary>
    /// Activates this stage.
    /// </summary>
    /// <param name="auditContext">The audit context.</param>
    public void Activate(IAuditContext auditContext)
    {
        if (IsActive)
        {
            throw new InvalidOperationException("Stage is already active.");
        }

        IsActive = true;
        SetUpdated(auditContext);
    }

    /// <summary>
    /// Deactivates this stage.
    /// </summary>
    /// <param name="auditContext">The audit context.</param>
    public void Deactivate(IAuditContext auditContext)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Stage is already inactive.");
        }

        IsActive = false;
        SetUpdated(auditContext);
    }

    /// <summary>
    /// Calculates the days remaining until this stage ends.
    /// </summary>
    /// <param name="currentDate">The current date to calculate from.</param>
    /// <returns>The number of days remaining, or 0 if the stage has ended.</returns>
    public int GetDaysRemaining(DateTime currentDate)
    {
        if (currentDate >= EndDate)
        {
            return 0;
        }

        var daysRemaining = (EndDate - currentDate).Days;
        return Math.Max(0, daysRemaining);
    }

    /// <summary>
    /// Checks if this stage is currently active and within its period.
    /// </summary>
    /// <param name="currentDate">The current date to check against.</param>
    /// <returns>True if the stage is both active and within its period.</returns>
    public bool IsCurrent(DateTime currentDate)
    {
        return IsActive && IsWithinPeriod(currentDate);
    }

    /// <summary>
    /// Checks if the given date falls within this stage's period.
    /// </summary>
    /// <param name="currentDate">The date to check.</param>
    /// <returns>True if the date is within the stage period, false otherwise.</returns>
    public bool IsWithinPeriod(DateTime currentDate)
    {
        return currentDate >= StartDate && currentDate <= EndDate;
    }

    /// <summary>
    /// Updates the dates for this stage.
    /// </summary>
    /// <param name="startDate">The new start date.</param>
    /// <param name="endDate">The new end date.</param>
    /// <param name="auditContext">The audit context.</param>
    public void UpdateDates(DateTime startDate, DateTime endDate, IAuditContext auditContext)
    {
        if (startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date.");
        }

        StartDate = startDate;
        EndDate = endDate;
        SetUpdated(auditContext);
    }

    /// <summary>
    /// Updates the stage details.
    /// </summary>
    /// <param name="title">The new title (optional).</param>
    /// <param name="description">The new description (optional).</param>
    /// <param name="auditContext">The audit context.</param>
    public void UpdateDetails(string? title, string? description, IAuditContext auditContext)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            if (title.Length > 200)
            {
                throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));
            }

            Title = title.Trim();
        }

        if (description != null)
        {
            if (description.Length > 2000)
            {
                throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(description));
            }

            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        SetUpdated(auditContext);
    }
}
