using LinaSys.Shared.Domain.Constants;

namespace LinaSys.Web.Helpers;

/// <summary>
/// Helper class for consistent role visualization across the application.
/// </summary>
public static class RoleBadgeHelper
{
    /// <summary>
    /// Gets the Phoenix badge CSS class for a specific role.
    /// </summary>
    /// <returns></returns>
    public static string GetBadgeClass(string role) => role switch
    {
        Roles.GlobalAdministrator => "badge-phoenix-warning",
        Roles.Administrator => "badge-phoenix-danger",
        Roles.Coordinator => "badge-phoenix-primary",
        Roles.Mentor => "badge-phoenix-success",
        Roles.Guide => "badge-phoenix-info",
        Roles.Facilitator => "badge-phoenix-secondary",
        Roles.Starter => "badge-phoenix-light",
        _ => "badge-phoenix-secondary"
    };

    /// <summary>
    /// Gets the display name in Spanish for a specific role.
    /// </summary>
    /// <returns></returns>
    public static string GetRoleDisplayName(string role) => role switch
    {
        Roles.GlobalAdministrator => "Admin Global",
        Roles.Administrator => "Administrador",
        Roles.Coordinator => "Coordinador",
        Roles.Mentor => "Mentor",
        Roles.Guide => "Guía",
        Roles.Facilitator => "Facilitador",
        Roles.Starter => "Emprendedor",
        _ => role
    };

    /// <summary>
    /// Gets the abbreviated display name for a role (for compact displays).
    /// </summary>
    /// <returns></returns>
    public static string GetRoleAbbreviation(string role) => role switch
    {
        Roles.GlobalAdministrator => "GA",
        Roles.Administrator => "ADM",
        Roles.Coordinator => "COORD",
        Roles.Mentor => "MEN",
        Roles.Guide => "GU",
        Roles.Facilitator => "FAC",
        Roles.Starter => "EMP",
        _ => role.Length > 3 ? role.Substring(0, 3).ToUpper() : role.ToUpper()
    };

    /// <summary>
    /// Gets the icon class for a specific role.
    /// </summary>
    /// <returns></returns>
    public static string GetRoleIcon(string role) => role switch
    {
        Roles.GlobalAdministrator => "fas fa-crown",
        Roles.Administrator => "fas fa-user-shield",
        Roles.Coordinator => "fas fa-user-tie",
        Roles.Mentor => "fas fa-chalkboard-teacher",
        Roles.Guide => "fas fa-compass",
        Roles.Facilitator => "fas fa-hands-helping",
        Roles.Starter => "fas fa-rocket",
        _ => "fas fa-user"
    };

    /// <summary>
    /// Determines the hierarchy level of a role (lower number = higher authority).
    /// </summary>
    /// <returns></returns>
    public static int GetRoleHierarchy(string role) => role switch
    {
        Roles.GlobalAdministrator => 1,
        Roles.Administrator => 2,
        Roles.Coordinator => 3,
        Roles.Mentor => 4,
        Roles.Guide => 5,
        Roles.Facilitator => 6,
        Roles.Starter => 7,
        _ => 99
    };
}
