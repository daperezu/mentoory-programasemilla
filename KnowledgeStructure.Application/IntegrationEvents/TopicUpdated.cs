using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.KnowledgeStructure.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a topic is updated.
/// This event is used to synchronize project topics that are based on this source.
/// </summary>
public sealed record TopicUpdated(
    long TopicId,
    string Name,
    int Order) : IntegrationEvent, INotification;
