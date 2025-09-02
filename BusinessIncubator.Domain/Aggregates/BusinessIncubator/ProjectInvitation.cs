using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Represents an invitation for a user to join a project.
/// </summary>
public class ProjectInvitation : SoftDeletableEntity
{
    protected ProjectInvitation()
    {
    }

    // Private constructor for the Create method
    private ProjectInvitation(
        long projectId,
        string email,
        string fullName,
        string identificationNumber,
        string role,
        string invitationToken,
        DateTime expiresAt)
    {
        ExternalId = Guid.NewGuid();
        ProjectId = projectId;
        Email = email;
        FullName = fullName;
        IdentificationNumber = identificationNumber;
        Role = role;
        InvitationToken = invitationToken;
        ExpiresAt = expiresAt;
        Status = ProjectInvitationStatus.Pending;
    }

    /// <summary>
    /// Gets the external identifier for this invitation.
    /// </summary>
    public Guid ExternalId { get; private set; }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// Gets the email address of the invited user.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the full name of the invited user.
    /// </summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the identification number of the invited user.
    /// </summary>
    public string IdentificationNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the role ID assigned to the user in the project.
    /// </summary>
    public string Role { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the invitation token used for accepting the invitation.
    /// </summary>
    public string InvitationToken { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the status of the invitation.
    /// </summary>
    public ProjectInvitationStatus Status { get; private set; } = ProjectInvitationStatus.Pending;

    /// <summary>
    /// Gets the expiration date of the invitation.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the date when the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; private set; }

    /// <summary>
    /// Navigation property for EF Core.
    /// </summary>
    internal virtual Project Project { get; private set; } = null!;

    /// <summary>
    /// Accepts the invitation.
    /// </summary>
    /// <param name="acceptedAt">The acceptance timestamp.</param>
    /// <returns>True if the invitation was successfully accepted, false otherwise.</returns>
    public bool Accept(DateTime acceptedAt)
    {
        if (Status != ProjectInvitationStatus.Pending)
        {
            return false;
        }

        if (acceptedAt > ExpiresAt)
        {
            Status = ProjectInvitationStatus.Expired;
            return false;
        }

        Status = ProjectInvitationStatus.Accepted;
        AcceptedAt = acceptedAt;
        return true;
    }

    /// <summary>
    /// Declines the invitation.
    /// </summary>
    /// <param name="declinedAt">The decline timestamp.</param>
    /// <returns>True if the invitation was successfully declined, false otherwise.</returns>
    public bool Decline(DateTime declinedAt)
    {
        if (Status != ProjectInvitationStatus.Pending)
        {
            return false;
        }

        if (declinedAt > ExpiresAt)
        {
            Status = ProjectInvitationStatus.Expired;
            return false;
        }

        Status = ProjectInvitationStatus.Declined;
        return true;
    }

    /// <summary>
    /// Revokes the invitation.
    /// </summary>
    /// <returns>True if the invitation was successfully revoked, false otherwise.</returns>
    public bool Revoke()
    {
        if (Status != ProjectInvitationStatus.Pending)
        {
            return false;
        }

        Status = ProjectInvitationStatus.Revoked;
        return true;
    }

    /// <summary>
    /// Checks if the invitation has expired.
    /// </summary>
    /// <param name="currentTime">The current timestamp to check against.</param>
    /// <returns>True if the invitation has expired, false otherwise.</returns>
    public bool IsExpired(DateTime currentTime)
    {
        return currentTime > ExpiresAt && Status == ProjectInvitationStatus.Pending;
    }

    /// <summary>
    /// Marks the invitation as expired.
    /// </summary>
    /// <param name="currentTime">The current timestamp.</param>
    public void MarkAsExpired(DateTime currentTime)
    {
        if (Status == ProjectInvitationStatus.Pending && IsExpired(currentTime))
        {
            Status = ProjectInvitationStatus.Expired;
        }
    }

    /// <summary>
    /// Creates a new project invitation with proper audit information.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="email">The email address.</param>
    /// <param name="fullName">The full name.</param>
    /// <param name="identificationNumber">The identification number.</param>
    /// <param name="role">The role ID.</param>
    /// <param name="expirationDays">The number of days until expiration.</param>
    /// <param name="auditContext">The audit context.</param>
    /// <returns>A new project invitation.</returns>
    internal static ProjectInvitation Create(
        long projectId,
        string email,
        string fullName,
        string identificationNumber,
        string? role,
        int expirationDays,
        IAuditContext auditContext)
    {
        var invitation = new ProjectInvitation(
            projectId,
            email.Trim().ToLowerInvariant(),
            fullName.Trim(),
            identificationNumber.Trim(),
            role?.Trim() ?? string.Empty,
            Guid.NewGuid().ToString("N"),
            auditContext.UtcNow.AddDays(expirationDays));

        invitation.SetCreated(auditContext);
        return invitation;
    }
}
