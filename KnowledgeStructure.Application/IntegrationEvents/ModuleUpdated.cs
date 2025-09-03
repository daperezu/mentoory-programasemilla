using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.KnowledgeStructure.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a module is updated.
/// This event is used to synchronize project modules that are based on this source.
/// </summary>
public sealed record ModuleUpdated(
    long ModuleId,
    string Name,
    int Order) : IntegrationEvent, INotification;
