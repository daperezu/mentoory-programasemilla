using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.KnowledgeStructure.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a subject is deleted.
/// This event is used to clear source references in project subjects.
/// </summary>
public sealed record SubjectDeleted(
    long SubjectId) : IntegrationEvent, INotification;
