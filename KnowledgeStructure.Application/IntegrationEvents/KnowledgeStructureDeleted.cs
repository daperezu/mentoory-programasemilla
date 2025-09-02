using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.KnowledgeStructure.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a knowledge structure is deleted.
/// This event is used to clear source references in project knowledge structures.
/// </summary>
public sealed record KnowledgeStructureDeleted(
    long StructureId) : IntegrationEvent, INotification;
