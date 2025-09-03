namespace LinaSys.Shared.Domain.SeedWork;

/// <summary>
/// Abstract base class for entities that need to track audit information.
/// </summary>
public abstract class AuditableEntity : Entity
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Gets or sets the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; protected set; }

    /// <summary>
    /// Sets the creation audit information.
    /// </summary>
    /// <param name="auditableContext">The context containing audit information.</param>
    protected void SetCreated(IAuditContext auditableContext)
    {
        if (auditableContext is null)
        {
            throw new ArgumentNullException(nameof(auditableContext), "Audit context cannot be null.");
        }

        if (auditableContext.UtcNow == default)
        {
            throw new InvalidOperationException("UtcNow must be set in the audit context.");
        }

        if (auditableContext.User is null)
        {
            throw new InvalidOperationException("User must be set in the audit context.");
        }

        if (CreatedAt != default)
        {
            throw new InvalidOperationException("CreatedAt has already been set for this entity.");
        }

        CreatedAt = auditableContext.UtcNow;
        CreatedBy = auditableContext.User;
    }

    /// <summary>
    /// Sets the update audit information.
    /// </summary>
    /// <param name="auditableContext">The context containing audit information.</param>
    protected void SetUpdated(IAuditContext auditableContext)
    {
        if (auditableContext is null)
        {
            throw new ArgumentNullException(nameof(auditableContext), "Audit context cannot be null.");
        }

        if (CreatedAt == default)
        {
            throw new InvalidOperationException("CreatedAt must be set before updating the entity.");
        }

        if (auditableContext.UtcNow < CreatedAt)
        {
            throw new InvalidOperationException("UpdatedAt cannot be earlier than CreatedAt.");
        }

        if (UpdatedAt.HasValue && auditableContext.UtcNow < UpdatedAt.Value)
        {
            throw new InvalidOperationException("UpdatedAt must be greater than the previous UpdatedAt value.");
        }

        UpdatedAt = auditableContext.UtcNow;
        UpdatedBy = auditableContext.User ?? throw new InvalidOperationException("User must be set in the audit context.");
    }
}
