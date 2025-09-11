using LinaSys.Auth.Application.Queries;
using LinaSys.BusinessIncubator.Application.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Approve;

/// <summary>
/// Handler for approving form submissions.
/// </summary>
public sealed class ApproveFormSubmissionCommandHandler(
    IBusinessIncubatorRepository repository,
    IMediator mediator,
    ILogger<ApproveFormSubmissionCommandHandler> logger,
    ITimeProvider timeProvider,
    IApplicationUrlService urlService) : BaseCommandHandler<ApproveFormSubmissionCommand>
{

    public override async Task<Result> Handle(ApproveFormSubmissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
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

            // Approve the submission
            submission.Approve(request.ApproverUserId, timeProvider.UtcNow);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // Publish integration event to create DiagnosisAnswers (with dual answers support)
            var integrationEvent = new ProjectFormSubmissionApproved(
                ProjectId: submission.ProjectId,
                SubmissionId: submission.Id,
                ParticipantUserId: submission.ParticipantUserId,
                CoordinatorUserId: submission.CoordinatorUserId ?? request.ApproverUserId,
                StarterDraftData: submission.DraftData ?? string.Empty,
                CoordinatorDraftData: submission.CoordinatorData ?? string.Empty,
                Phase: submission.Phase,
                ApprovedAt: submission.ApprovedAt!.Value,
                ApprovedByUserId: submission.ApprovedByUserId!);

            await mediator.Publish(integrationEvent, cancellationToken);

            logger.LogInformation(
                "Form approved by {ApproverUserId} for participant {ParticipantUserId} in project {ProjectId}",
                request.ApproverUserId,
                submission.ParticipantUserId,
                request.ProjectId);

            // Send approval notification email
            await SendApprovalNotification(project, submission, cancellationToken);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation approving form");
            return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("BusinessRule", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving form for project {ProjectId}", request.ProjectId);
            return Failure(ResultErrorCodes.Unknown, ("Approve", "Error al aprobar el formulario. Por favor, intenta nuevamente."));
        }
    }

    private async Task SendApprovalNotification(
        Domain.Aggregates.BusinessIncubator.Project project,
        Domain.Aggregates.BusinessIncubator.ProjectFormSubmission submission,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get current user information from Identity
            var userResult = await mediator.Send(new GetUserByIdQuery(submission.ParticipantUserId), cancellationToken);
            if (!userResult.IsSuccess)
            {
                logger.LogWarning("Could not find user details for user {UserId}", submission.ParticipantUserId);
                return;
            }

            var userDetails = userResult.Value;
            if (userDetails is null)
            {
                logger.LogWarning("User details are null for user {UserId}", submission.ParticipantUserId);
                return;
            }

            // Get project and business incubator info for dashboard URL
            var businessIncubator = await repository.GetByIdAsync(project.BusinessIncubatorId, cancellationToken);
            if (businessIncubator is null)
            {
                logger.LogWarning("Could not find business incubator {BusinessIncubatorId}", project.BusinessIncubatorId);
                return;
            }

            var dashboardUrl = urlService.GetProjectDashboardUrl(businessIncubator.ExternalId, project.ExternalId);

            // Publish new integration event for form approval
            var integrationEvent = new FormApprovedIntegrationEvent(
                SubmissionId: (int)submission.Id,
                ProjectId: (int)project.Id,
                ProjectName: project.Name,
                ParticipantUserId: submission.ParticipantUserId,
                ParticipantEmail: userDetails.Email,
                ParticipantName: userDetails.FullName ?? userDetails.UserName,
                DashboardUrl: dashboardUrl,
                OccurredOn: timeProvider.UtcNow);

            await mediator.Publish(integrationEvent, cancellationToken);
            logger.LogInformation("Published FormApprovedIntegrationEvent for submission {SubmissionId}", submission.Id);
        }
        catch (Exception ex)
        {
            // Don't fail the approval if email sending fails
            logger.LogError(ex, "Failed to send approval notification email for submission {SubmissionId}", submission.Id);
        }
    }
}
