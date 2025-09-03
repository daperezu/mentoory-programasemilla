using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.Diagnostics.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a question is updated.
/// This event is used to synchronize project questions that are based on this source.
/// </summary>
public sealed record QuestionUpdated(
    long QuestionId,
    string Text,
    int AnswerType,
    int AppliesToPhase,
    bool IsUsedForMentoringPlan,
    bool IsUsedForDiagnosis,
    int Order) : IntegrationEvent, INotification;
