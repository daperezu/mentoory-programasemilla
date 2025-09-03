namespace LinaSys.Permissions.Domain.Constants;

/// <summary>
/// Defines the types of resources that can be protected in the system.
/// </summary>
public static class ResourceTypes
{
    /// <summary>
    /// Web features (controller actions) that require authorization.
    /// </summary>
    public const int WebFeature = 1;

    /// <summary>
    /// Business incubators that users can have access to.
    /// </summary>
    public const int BusinessIncubator = 2;

    /// <summary>
    /// Projects within business incubators that require specific access.
    /// </summary>
    public const int Project = 3;

    /// <summary>
    /// Diagnosis forms used in project assessment.
    /// </summary>
    public const int DiagnosisForm = 4;

    /// <summary>
    /// Menu items that can be displayed based on user permissions.
    /// </summary>
    public const int MenuItem = 5;

    /// <summary>
    /// Gets the display name for a resource type.
    /// </summary>
    /// <param name="resourceType">The resource type constant.</param>
    /// <returns>The display name for the resource type.</returns>
    public static string GetDisplayName(int resourceType)
    {
        return resourceType switch
        {
            WebFeature => "Web Feature",
            BusinessIncubator => "Business Incubator",
            Project => "Project",
            DiagnosisForm => "Diagnosis Form",
            MenuItem => "Menu Item",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Validates if a resource type is valid.
    /// </summary>
    /// <param name="resourceType">The resource type to validate.</param>
    /// <returns>True if the resource type is valid, false otherwise.</returns>
    public static bool IsValid(int resourceType)
    {
        return resourceType is WebFeature or BusinessIncubator or Project or DiagnosisForm or MenuItem;
    }
}
