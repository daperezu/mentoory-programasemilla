using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.KnowledgeStructure.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a knowledge structure is updated.
/// This event is used to synchronize project knowledge structures that are based on this source.
/// </summary>
public sealed record KnowledgeStructureUpdated(
    long StructureId,
    string Name,
    string? Description) : IntegrationEvent, INotification;
