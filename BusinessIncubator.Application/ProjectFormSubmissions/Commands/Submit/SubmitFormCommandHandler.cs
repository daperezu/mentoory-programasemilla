using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Submit;

/// <summary>
/// Handler for submitting forms for review.
/// </summary>
public sealed class SubmitFormCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<SubmitFormCommandHandler> logger,
    IPublisher publisher,
    ITimeProvider timeProvider) : BaseCommandHandler<SubmitFormCommand>
{

    public override async Task<Result> Handle(SubmitFormCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get project with form submissions and invitations
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

            // Verify ownership
            if (submission.ParticipantUserId != request.ParticipantUserId)
            {
                return Failure(ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource, (nameof(request.ParticipantUserId), "No tienes permiso para enviar este formulario."));
            }

            // Submit the form
            submission.Submit(timeProvider.UtcNow);

            // Save changes
            await repository.UpdateAsync(project, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Form submitted by participant {ParticipantUserId} in project {ProjectId}, submission {SubmissionId}",
                request.ParticipantUserId,
                request.ProjectId,
                request.SubmissionId);

            // Get participant information for the integration event
            var participantInvitation = project.ProjectInvitations
                .FirstOrDefault(i => i.IdentificationNumber == submission.ParticipantUserId.ToString() &&
                                   i.Status == Domain.Enums.ProjectInvitationStatus.Accepted);

            if (participantInvitation is not null)
            {
                // Publish integration event for form submission
                var now = timeProvider.UtcNow;
                var integrationEvent = new FormSubmittedIntegrationEvent(
                    SubmissionId: (int)submission.Id,
                    ProjectId: (int)project.Id,
                    ProjectName: project.Name,
                    ParticipantUserId: submission.ParticipantUserId.ToString(),
                    ParticipantEmail: participantInvitation.Email,
                    ParticipantName: participantInvitation.FullName,
                    SubmittedAt: submission.SubmittedAt ?? now,
                    OccurredOn: now);

                await publisher.Publish(integrationEvent, cancellationToken);
                logger.LogInformation("Published FormSubmittedIntegrationEvent for submission {SubmissionId}", submission.Id);
            }

            // Email notifications are now handled by the FormSubmittedHandler via integration event
            return Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation submitting form");
            return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("BusinessRule", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting form for project {ProjectId}", request.ProjectId);
            return Failure(ResultErrorCodes.Unknown, ("Submit", "Error al enviar el formulario. Por favor, intenta nuevamente."));
        }
    }
}