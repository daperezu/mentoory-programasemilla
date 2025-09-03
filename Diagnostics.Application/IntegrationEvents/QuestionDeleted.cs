using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.Diagnostics.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a question is deleted.
/// This event is used to clear source references in project questions.
/// </summary>
public sealed record QuestionDeleted(
    long QuestionId) : IntegrationEvent, INotification;
