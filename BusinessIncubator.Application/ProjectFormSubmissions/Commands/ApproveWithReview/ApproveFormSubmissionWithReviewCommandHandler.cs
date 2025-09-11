using LinaSys.Auth.Application.Queries;
using LinaSys.BusinessIncubator.Application.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.ApproveWithReview;

/// <summary>
/// Handler for approving form submissions with review record creation.
/// This ensures both audit trail (review) and actual approval happen in the same transaction.
/// </summary>
public sealed class ApproveFormSubmissionWithReviewCommandHandler(
    IBusinessIncubatorRepository repository,
    IMediator mediator,
    ILogger<ApproveFormSubmissionWithReviewCommandHandler> logger,
    ITimeProvider timeProvider,
    IApplicationUrlService urlService,
    IAuditContext auditContext) : BaseCommandHandler<ApproveFormSubmissionWithReviewCommand>
{
    public override async Task<Result> Handle(
        ApproveFormSubmissionWithReviewCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get project with form submissions
            var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectId), "El proyecto no existe."));
            }

            // 2. Get the submission
            var submission = project.GetFormSubmission(request.SubmissionId);
            if (submission is null)
            {
                return Failure(ResultErrorCodes.DiagnosisForm_NotFound, (nameof(request.SubmissionId), "El formulario no existe."));
            }

            // 3. Check if already approved (idempotency)
            if (submission.Status == ProjectFormSubmissionStatus.Approved)
            {
                logger.LogInformation(
                    "Submission {SubmissionId} is already approved. Returning success for idempotency.",
                    request.SubmissionId);
                return Success();
            }

            // 3.5. Validate coordinator has provided answers
            if (string.IsNullOrWhiteSpace(submission.CoordinatorData))
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    ("CoordinatorAnswers", "Debe completar su revisión antes de aprobar el formulario."));
            }

            // 4. Create or update review record for audit trail
            var review = await repository.GetReviewBySubmissionIdAsync(request.SubmissionId, cancellationToken);
            if (review is null)
            {
                // Create new review
                review = new ProjectFormReview(
                    request.SubmissionId,
                    request.ApproverUserId,
                    ReviewStatus.Approved,
                    timeProvider.UtcNow,
                    request.Comments);

                review.CreatedAt = auditContext.UtcNow;
                review.CreatedBy = request.ApproverUserId;

                await repository.AddReviewAsync(review, cancellationToken);
            }
            else
            {
                // Update existing review
                review.Approve(request.Comments);
                review.UpdatedAt = auditContext.UtcNow;
                review.UpdatedBy = request.ApproverUserId;

                await repository.UpdateReviewAsync(review, cancellationToken);
            }

            // 5. Approve the submission (actual status change)
            submission.Approve(request.ApproverUserId, timeProvider.UtcNow);

            // 6. Save all changes in one transaction
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Publish integration event for diagnostics domain (with dual answers)
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
                "Form submission {SubmissionId} approved by {ApproverUserId} for participant {ParticipantUserId} in project {ProjectId}",
                request.SubmissionId,
                request.ApproverUserId,
                submission.ParticipantUserId,
                request.ProjectId);

            // 8. Send approval notification email (don't fail if this fails)
            await SendApprovalNotificationAsync(project, submission, cancellationToken);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation approving form submission {SubmissionId}", request.SubmissionId);
            return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("BusinessRule", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving form submission {SubmissionId} for project {ProjectId}",
                request.SubmissionId, request.ProjectId);
            return Failure(ResultErrorCodes.Unknown, ("Approve", "Error al aprobar el formulario. Por favor, intenta nuevamente."));
        }
    }

    private async Task SendApprovalNotificationAsync(
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

            // Get business incubator info for dashboard URL
            var businessIncubator = await repository.GetByIdAsync(project.BusinessIncubatorId, cancellationToken);
            if (businessIncubator is null)
            {
                logger.LogWarning("Could not find business incubator {BusinessIncubatorId}", project.BusinessIncubatorId);
                return;
            }

            var dashboardUrl = urlService.GetProjectDashboardUrl(businessIncubator.ExternalId, project.ExternalId);

            // Publish integration event for email notification
            var emailEvent = new FormApprovedIntegrationEvent(
                SubmissionId: (int)submission.Id,
                ProjectId: (int)project.Id,
                ProjectName: project.Name,
                ParticipantUserId: submission.ParticipantUserId,
                ParticipantEmail: userDetails.Email,
                ParticipantName: userDetails.FullName ?? userDetails.UserName,
                DashboardUrl: dashboardUrl,
                OccurredOn: timeProvider.UtcNow);

            await mediator.Publish(emailEvent, cancellationToken);
            logger.LogInformation("Published FormApprovedIntegrationEvent for submission {SubmissionId}", submission.Id);
        }
        catch (Exception ex)
        {
            // Don't fail the approval if email sending fails
            logger.LogError(ex, "Failed to send approval notification email for submission {SubmissionId}", submission.Id);
        }
    }
}