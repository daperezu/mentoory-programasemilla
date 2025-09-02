namespace LinaSys.Shared.Domain.SeedWork;

/// <summary>
/// Abstract base class for entities that support soft deletion.
/// </summary>
public abstract class SoftDeletableEntity : AuditableEntity
{
    /// <summary>
    /// Gets or sets the date and time when the entity was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// Gets or sets the user who deleted the entity.
    /// </summary>
    public string? DeletedBy { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was restored.
    /// </summary>
    public DateTime? RestoredAt { get; protected set; }

    /// <summary>
    /// Gets or sets the user who restored the entity.
    /// </summary>
    public string? RestoredBy { get; protected set; }

    /// <summary>
    /// Ensures that the entity is not deleted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the entity is deleted.</exception>
    public void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("This entity has already been deleted.");
        }
    }

    /// <summary>
    /// Sets the deletion audit information.
    /// </summary>
    /// <param name="auditableContext">The context containing audit information.</param>
    public void SetDeleted(IAuditContext auditableContext)
    {
        EnsureNotDeleted();

        DeletedAt = auditableContext.UtcNow;
        DeletedBy = auditableContext.User;
        IsDeleted = true;

        SetUpdated(auditableContext);
    }

    /// <summary>
    /// Sets the restoration audit information.
    /// </summary>
    /// <param name="auditableContext">The context containing audit information.</param>
    public void SetRestored(IAuditContext auditableContext)
    {
        if (!IsDeleted)
        {
            throw new InvalidOperationException("This entity was not deleted. Cannot be restored.");
        }

        RestoredAt = auditableContext.UtcNow;
        RestoredBy = auditableContext.User;
        IsDeleted = false;

        SetUpdated(auditableContext);
    }
}
