using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.KnowledgeStructure.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a subject is updated.
/// This event is used to synchronize project subjects that are based on this source.
/// </summary>
public sealed record SubjectUpdated(
    long SubjectId,
    string Title,
    string? Content,
    int Order) : IntegrationEvent, INotification;
