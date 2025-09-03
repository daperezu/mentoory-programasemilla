using System.Text.Json;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Represents a report template for generating project reports.
/// </summary>
public class ReportTemplate : Entity, IAggregateRoot
{
    private readonly List<ReportSchedule> _schedules = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportTemplate"/> class.
    /// </summary>
    /// <param name="name">The template name.</param>
    /// <param name="description">The template description.</param>
    /// <param name="type">The report type.</param>
    /// <param name="isGlobal">Whether the template is global or project-specific.</param>
    /// <param name="projectId">The project ID (null for global templates).</param>
    /// <param name="configuration">The report configuration as JSON.</param>
    /// <param name="createdBy">The user creating the template.</param>
    public ReportTemplate(
        string name,
        string? description,
        ReportType type,
        bool isGlobal,
        long? projectId,
        string configuration,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Template name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(configuration))
        {
            throw new ArgumentException("Template configuration is required.", nameof(configuration));
        }

        if (!isGlobal && !projectId.HasValue)
        {
            throw new ArgumentException("Project-specific templates must have a project ID.", nameof(projectId));
        }

        if (isGlobal && projectId.HasValue)
        {
            throw new ArgumentException("Global templates cannot have a project ID.", nameof(projectId));
        }

        // Validate JSON configuration
        ValidateConfiguration(configuration);

        ExternalId = Guid.NewGuid();
        Name = name;
        Description = description;
        Type = type;
        IsGlobal = isGlobal;
        ProjectId = projectId;
        ConfigurationJson = configuration;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportTemplate"/> class.
    /// </summary>
    protected ReportTemplate()
    {
    }

    /// <summary>
    /// Gets the external ID.
    /// </summary>
    public Guid ExternalId { get; private set; }

    /// <summary>
    /// Gets the template name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the template description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the report type.
    /// </summary>
    public ReportType Type { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the template is global.
    /// </summary>
    public bool IsGlobal { get; private set; }

    /// <summary>
    /// Gets the project ID for project-specific templates.
    /// </summary>
    public long? ProjectId { get; private set; }

    /// <summary>
    /// Gets the configuration JSON.
    /// </summary>
    public string ConfigurationJson { get; private set; } = string.Empty;

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
    /// Gets the navigation property to the project.
    /// </summary>
    public virtual Project? Project { get; private set; }

    /// <summary>
    /// Gets the report schedules for this template.
    /// </summary>
    public IReadOnlyList<ReportSchedule> Schedules => _schedules.AsReadOnly();

    /// <summary>
    /// Updates the template information.
    /// </summary>
    /// <param name="name">The new template name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="configuration">The new configuration JSON.</param>
    /// <param name="updatedBy">The user updating the template.</param>
    public void Update(string name, string? description, string configuration, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Template name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(configuration))
        {
            throw new ArgumentException("Template configuration is required.", nameof(configuration));
        }

        // Validate JSON configuration
        ValidateConfiguration(configuration);

        Name = name;
        Description = description;
        ConfigurationJson = configuration;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new report schedule for this template.
    /// </summary>
    /// <param name="cronExpression">The CRON expression for scheduling.</param>
    /// <param name="recipients">The list of recipient email addresses.</param>
    /// <param name="createdBy">The user creating the schedule.</param>
    /// <returns>The created report schedule.</returns>
    public ReportSchedule AddSchedule(string cronExpression, List<string> recipients, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentException("CRON expression is required.", nameof(cronExpression));
        }

        if (recipients == null || recipients.Count == 0)
        {
            throw new ArgumentException("At least one recipient is required.", nameof(recipients));
        }

        // Validate email addresses
        foreach (var email in recipients)
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                throw new ArgumentException($"Invalid email address: {email}", nameof(recipients));
            }
        }

        var schedule = new ReportSchedule(
            this.Id,
            cronExpression,
            recipients,
            createdBy);

        _schedules.Add(schedule);
        return schedule;
    }

    /// <summary>
    /// Deactivates all schedules for this template.
    /// </summary>
    /// <param name="updatedBy">The user deactivating the schedules.</param>
    public void DeactivateSchedules(string updatedBy)
    {
        foreach (var schedule in _schedules.Where(s => s.IsActive))
        {
            schedule.Deactivate(updatedBy);
        }
    }

    /// <summary>
    /// Validates that the template can be deleted.
    /// </summary>
    /// <returns>True if the template can be deleted.</returns>
    public bool CanDelete()
    {
        // Global templates cannot be deleted by coordinators
        if (IsGlobal)
        {
            return false;
        }

        // Check if there are any active schedules
        return !_schedules.Any(s => s.IsActive);
    }

    /// <summary>
    /// Validates the configuration JSON.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    private static void ValidateConfiguration(string configuration)
    {
        try
        {
            JsonDocument.Parse(configuration);
        }
        catch (JsonException)
        {
            throw new ArgumentException("Configuration must be valid JSON.", nameof(configuration));
        }
    }

    /// <summary>
    /// Validates an email address format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if valid email format.</returns>
    private static bool IsValidEmail(string email)
    {
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