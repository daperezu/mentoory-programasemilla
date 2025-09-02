using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.KnowledgeStructure.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a module is deleted.
/// This event is used to clear source references in project modules.
/// </summary>
public sealed record ModuleDeleted(
    long ModuleId) : IntegrationEvent, INotification;
