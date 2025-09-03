using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.Diagnostics.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when an answer option is deleted.
/// This event is used to clear source references in project answer options.
/// </summary>
public sealed record AnswerOptionDeleted(
    long OptionId) : IntegrationEvent, INotification;
