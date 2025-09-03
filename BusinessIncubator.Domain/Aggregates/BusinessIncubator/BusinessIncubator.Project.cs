using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Partial class for managing projects within a Business Incubator.
/// </summary>
public partial class BusinessIncubator
{
    /// <summary>
    /// Adds a new project to the Business Incubator.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="description">The description of the project.</param>
    /// <param name="key">The unique key for the project.</param>
    /// <param name="auditContext">The context containing audit information.</param>
    /// <returns>The external ID of the newly added project.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a project with the same key already exists in the incubator.</exception>
    public Guid AddProject(string name, string? description, string key, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        if (Projects.Any(p => p.Key == key))
        {
            throw new InvalidOperationException($"A project with key '{key}' already exists in this incubator.");
        }

        var project = new Project(name, description, key, this.Id, auditContext);
        Projects.Add(project);
        return project.ExternalId;
    }

    /// <summary>
    /// Checks if there is a duplicate project name or key within the Business Incubator.
    /// </summary>
    /// <param name="projectExternalId">The external ID of the project to exclude from the check.</param>
    /// <param name="name">The name of the project to check.</param>
    /// <param name="key">The key of the project to check.</param>
    /// <returns>True if a duplicate project name or key exists, otherwise false.</returns>
    public bool HasDuplicateProjectNameOrKey(Guid projectExternalId, string name, string key)
    {
        return Projects.Any(p =>
            p.ExternalId != projectExternalId &&
            (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Marks a project as deleted within the Business Incubator.
    /// </summary>
    /// <param name="externalId">The external ID of the project to delete.</param>
    /// <param name="auditContext">The context containing audit information.</param>
    /// <exception cref="InvalidOperationException">Thrown if the project with the specified external ID is not found.</exception>
    public void SetProjectDeleted(Guid externalId, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var project = Projects.FirstOrDefault(p => p.ExternalId == externalId);
        if (project is null)
        {
            throw new InvalidOperationException($"Project with external ID '{externalId}' was not found.");
        }

        project.SetDeleted(auditContext);
    }

    /// <summary>
    /// Restores a previously deleted project within the Business Incubator.
    /// </summary>
    /// <param name="externalId">The external ID of the project to restore.</param>
    /// <param name="auditContext">The context containing audit information.</param>
    /// <exception cref="InvalidOperationException">Thrown if the project with the specified external ID is not found.</exception>
    public void SetProjectRestored(Guid externalId, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var project = Projects.FirstOrDefault(p => p.ExternalId == externalId);
        if (project is null)
        {
            throw new InvalidOperationException($"Project with external ID '{externalId}' was not found.");
        }

        project.SetRestored(auditContext);
    }

    /// <summary>
    /// Updates the details of an existing project within the Business Incubator.
    /// </summary>
    /// <param name="externalId">The external ID of the project to update.</param>
    /// <param name="name">The updated name of the project.</param>
    /// <param name="description">The updated description of the project.</param>
    /// <param name="key">The updated key of the project.</param>
    /// <param name="auditContext">The context containing audit information.</param>
    /// <exception cref="InvalidOperationException">Thrown if the project with the specified external ID is not found or is deleted.</exception>
    public void UpdateProject(Guid externalId, string name, string description, string key, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var project = Projects.FirstOrDefault(p => p.ExternalId == externalId && !p.IsDeleted);
        if (project is null)
        {
            throw new InvalidOperationException($"Project with ID '{externalId}' was not found.");
        }

        project.Update(name, description, key, auditContext);
    }
}
