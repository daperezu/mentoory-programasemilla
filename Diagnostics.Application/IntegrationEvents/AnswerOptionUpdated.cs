using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.Diagnostics.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when an answer option is updated.
/// This event is used to synchronize project answer options that are based on this source.
/// </summary>
public sealed record AnswerOptionUpdated(
    long OptionId,
    string Text,
    int Score,
    string Foda,
    string FodaExplanation,
    string Odsr,
    string OdsrExplanation,
    int Order,
    string? FollowUpQuestionText) : IntegrationEvent, INotification;
