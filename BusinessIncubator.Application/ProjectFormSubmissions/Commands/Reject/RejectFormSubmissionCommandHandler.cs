using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Reject;

/// <summary>
/// Handler for rejecting form submissions.
/// </summary>
public sealed class RejectFormSubmissionCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<RejectFormSubmissionCommandHandler> logger,
    IPublisher publisher,
    ITimeProvider timeProvider) : BaseCommandHandler<RejectFormSubmissionCommand>
{
    public override async Task<Result> Handle(RejectFormSubmissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate rejection reason
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, (nameof(request.RejectionReason), "Debe proporcionar una razón para el rechazo."));
            }

            // Get project with form submissions
            var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectId), "El proyecto no existe."));
            }

            // Get the submission
            var submission = project.GetFormSubmission(request.SubmissionId);
            if (submission is null)
            {
                return Failure(ResultErrorCodes.DiagnosisForm_NotFound, (nameof(request.SubmissionId), "El formulario no existe."));
            }

            // Reject the submission
            submission.Reject(request.RejectionReason, timeProvider.UtcNow);

            // Save changes
            await repository.UpdateAsync(project, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Form rejected by {ReviewerUserId} for participant {ParticipantUserId} in project {ProjectId}. Reason: {Reason}",
                request.ReviewerUserId,
                submission.ParticipantUserId,
                request.ProjectId,
                request.RejectionReason);

            // Get participant information for the integration event
            var participantInvitation = project.ProjectInvitations
                .FirstOrDefault(i => i.IdentificationNumber == submission.ParticipantUserId.ToString() &&
                                   i.Status == Domain.Enums.ProjectInvitationStatus.Accepted);

            if (participantInvitation is not null)
            {
                // Publish integration event for form rejection
                var integrationEvent = new FormRejectedIntegrationEvent(
                    SubmissionId: (int)submission.Id,
                    ProjectId: (int)project.Id,
                    ProjectName: project.Name,
                    ParticipantUserId: submission.ParticipantUserId.ToString(),
                    ParticipantEmail: participantInvitation.Email,
                    ParticipantName: participantInvitation.FullName,
                    Feedback: request.RejectionReason,
                    OccurredOn: timeProvider.UtcNow);

                await publisher.Publish(integrationEvent, cancellationToken);
                logger.LogInformation("Published FormRejectedIntegrationEvent for submission {SubmissionId}", submission.Id);
            }

            // Email notification is now handled by the FormRejectedHandler via integration event
            return Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation rejecting form");
            return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("BusinessRule", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting form for project {ProjectId}", request.ProjectId);
            return Failure(ResultErrorCodes.Unknown, ("Reject", "Error al rechazar el formulario. Por favor, intenta nuevamente."));
        }
    }
}