namespace LinaSys.Shared.Domain.Constants;

/// <summary>
/// Defines role constants used throughout the LinaSys system.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Starter role - participants who are starting their business journey.
    /// </summary>
    public const string Starter = "Starter";

    /// <summary>
    /// Coordinator role - manages business incubator projects and participants.
    /// </summary>
    public const string Coordinator = "Coordinator";

    /// <summary>
    /// Mentor role - provides guidance and support to participants.
    /// </summary>
    public const string Mentor = "Mentor";

    /// <summary>
    /// Guide role - assists participants with specific aspects of their journey.
    /// </summary>
    public const string Guide = "Guide";

    /// <summary>
    /// Facilitator role - facilitates workshops and training sessions.
    /// </summary>
    public const string Facilitator = "Facilitator";

    /// <summary>
    /// Liaison role - acts as a connection between different stakeholders.
    /// </summary>
    public const string Liaison = "Liaison";

    /// <summary>
    /// Administrator role - manages system configuration and users.
    /// </summary>
    public const string Administrator = "Administrator";

    /// <summary>
    /// Global Administrator role - has full system access across all incubators.
    /// </summary>
    public const string GlobalAdministrator = "Global Administrator";

    /// <summary>
    /// Gets all coordinator-allowed roles for bulk invitations.
    /// </summary>
    public static readonly string[] CoordinatorAllowedRoles =
    [
        Starter,
        Mentor,
        Guide,
        Liaison,
        Facilitator
    ];

    /// <summary>
    /// Gets all system roles.
    /// </summary>
    public static readonly string[] AllRoles =
    [
        Starter,
        Coordinator,
        Mentor,
        Guide,
        Facilitator,
        Liaison,
        Administrator,
        GlobalAdministrator
    ];
}
