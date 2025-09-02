namespace LinaSys.Shared.Domain.SeedWork;

/// <summary>
/// Represents the context for auditing operations, including the current user and the current UTC time.
/// </summary>
public interface IAuditContext
{
    /// <summary>
    /// Gets the user performing the operation.
    /// </summary>
    string? User { get; }

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
