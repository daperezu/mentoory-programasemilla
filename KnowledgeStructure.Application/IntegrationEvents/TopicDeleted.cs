using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.KnowledgeStructure.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a topic is deleted.
/// This event is used to clear source references in project topics.
/// </summary>
public sealed record TopicDeleted(
    long TopicId) : IntegrationEvent, INotification;
