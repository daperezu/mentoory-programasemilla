using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Application.IntegrationEvents;

/// <summary>
/// Integration event raised when a knowledge structure is updated.
/// </summary>
public record KnowledgeStructureUpdatedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// Gets or sets the knowledge structure ID.
    /// </summary>
    public long KnowledgeStructureId { get; set; }

    /// <summary>
    /// Gets or sets the knowledge structure name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the knowledge structure description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the source type (Diagnostics or KnowledgeStructure).
    /// </summary>
    public string SourceType { get; set; } = string.Empty;
}