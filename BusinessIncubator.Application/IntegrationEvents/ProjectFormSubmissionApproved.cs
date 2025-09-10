using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a project form submission is approved.
/// This event is used to create DiagnosisAnswers in the Diagnostics domain.
/// </summary>
public sealed record ProjectFormSubmissionApproved(
    long ProjectId,
    long SubmissionId,
    string ParticipantUserId,
    string DraftData,
    LinaSys.BusinessIncubator.Domain.Enums.QuestionPhase Phase,
    DateTime ApprovedAt,
    string ApprovedByUserId) : IntegrationEvent, INotification;