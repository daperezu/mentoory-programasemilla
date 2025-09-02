using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.Permissions.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a new protected resource needs to be created.
/// This event decouples the creation of business entities from permission management.
/// </summary>
public sealed record ProtectedResourceCreated : IntegrationEvent, INotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProtectedResourceCreated"/> class.
    /// </summary>
    /// <param name="externalId">The external ID of the resource.</param>
    /// <param name="resourceType">The type of the resource.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="creatorUserId">The ID of the user who created the resource (optional).</param>
    public ProtectedResourceCreated(Guid externalId, int resourceType, string name, string? creatorUserId = null)
    {
        ExternalId = externalId;
        ResourceType = resourceType;
        Name = name;
        CreatorUserId = creatorUserId;
    }

    /// <summary>
    /// The external ID of the resource that was created.
    /// </summary>
    public Guid ExternalId { get; init; }

    /// <summary>
    /// The type of the resource (from ResourceTypes constants).
    /// </summary>
    public int ResourceType { get; init; }

    /// <summary>
    /// The name of the resource.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The ID of the user who created the resource.
    /// If provided, this user will automatically get access to the resource.
    /// </summary>
    public string? CreatorUserId { get; init; }
}
