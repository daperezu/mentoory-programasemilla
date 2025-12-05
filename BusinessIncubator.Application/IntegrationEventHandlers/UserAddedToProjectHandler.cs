using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the UserAddedToProject integration event to automatically create
/// ProjectFormSubmission records for active form collection stages.
/// </summary>
/// <remarks>
/// This handler ensures users immediately see pending forms on their dashboard
/// when assigned to projects with active form collection stages, eliminating
/// the need for lazy creation via GetOrCreateFormSubmissionCommand.
/// Implements idempotency to prevent duplicate submissions on re-assignment.
/// </remarks>
public sealed class UserAddedToProjectHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<UserAddedToProjectHandler> logger)
    : INotificationHandler<UserAddedToProjectIntegrationEvent>
{
    public async Task Handle(
        UserAddedToProjectIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing UserAddedToProject event for user {UserId} ({UserEmail}) added to project {ProjectId} ({ProjectName}) with role {Role}",
            notification.UserId,
            notification.UserEmail,
            notification.ProjectId,
            notification.ProjectName,
            notification.Role);

        try
        {
            // Step 1: Validate role requires form submissions
            var rolesRequiringForms = new[] { Roles.Starter };
            if (!rolesRequiringForms.Contains(notification.Role))
            {
                logger.LogDebug(
                    "Role {Role} does not require automatic form creation for user {UserId} in project {ProjectId}",
                    notification.Role,
                    notification.UserId,
                    notification.ProjectId);
                return;
            }

            // Step 2: Fetch project with stages
            var project = await repository.GetProjectWithStagesAsync(
                notification.ProjectId,
                cancellationToken);

            if (project is null)
            {
                logger.LogWarning(
                    "Project {ProjectId} not found when processing UserAddedToProject for user {UserId}",
                    notification.ProjectId,
                    notification.UserId);
                return; // Graceful degradation - don't fail user assignment
            }

            // Step 3: Fetch knowledge structure for schema version
            var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(
                project.Id,
                cancellationToken);

            if (knowledgeStructure is null)
            {
                logger.LogWarning(
                    "Project {ProjectId} ({ProjectName}) has no knowledge structure. Cannot create form submissions for user {UserId}",
                    notification.ProjectId,
                    notification.ProjectName,
                    notification.UserId);
                return; // Can't create forms without structure
            }

            // Step 4: Find active form collection stages within their time window
            var currentDate = timeProvider.UtcNow;

            var activeFormStages = project.ProjectStages
                .Where(stage =>
                    stage.IsActive &&
                    (stage.Type == ProjectStageType.InitialFormCollection ||
                     stage.Type == ProjectStageType.FinalFormCollection) &&
                    stage.IsWithinPeriod(currentDate))
                .ToList();

            if (!activeFormStages.Any())
            {
                logger.LogInformation(
                    "No active form collection stages for project {ProjectId} ({ProjectName}). No form submissions created for user {UserId}",
                    notification.ProjectId,
                    notification.ProjectName,
                    notification.UserId);
                return; // Normal case - no active form stages
            }

            logger.LogInformation(
                "Found {Count} active form collection stage(s) for project {ProjectId}. Processing form submission creation for user {UserId}",
                activeFormStages.Count,
                notification.ProjectId,
                notification.UserId);

            // Step 5: Create form submissions for each active stage
            var formsCreated = 0;
            var formsAlreadyExisted = 0;

            foreach (var stage in activeFormStages)
            {
                try
                {
                    // Determine phase from stage type
                    var phase = ProjectFormSubmission.GetPhaseForStage(stage.Type);

                    if (phase == QuestionPhase.Undefined)
                    {
                        logger.LogWarning(
                            "Stage {StageId} ({StageTitle}) has type {StageType} which doesn't map to a form phase. Skipping form creation.",
                            stage.Id,
                            stage.Title,
                            stage.Type);
                        continue;
                    }

                    // Check if submission already exists (idempotency)
                    var existingSubmission = await repository.GetFormSubmissionAsync(
                        project.Id,
                        notification.UserId,
                        phase,
                        cancellationToken);

                    if (existingSubmission is not null)
                    {
                        logger.LogDebug(
                            "Form submission already exists for user {UserId}, project {ProjectId}, phase {Phase} (Status: {Status}). Skipping creation.",
                            notification.UserId,
                            notification.ProjectId,
                            phase,
                            existingSubmission.Status);
                        formsAlreadyExisted++;
                        continue; // Idempotency - don't create duplicate
                    }

                    // Create new form submission
                    var submission = ProjectFormSubmission.CreateForPhase(
                        projectId: project.Id,
                        participantUserId: notification.UserId,
                        formSchemaVersion: knowledgeStructure.CurrentVersion,
                        phase: phase,
                        projectStageId: stage.Id,
                        startedAt: currentDate);

                    repository.AddFormSubmission(submission);
                    formsCreated++;

                    logger.LogInformation(
                        "Created {Phase} form submission for user {UserId} ({UserEmail}) in project {ProjectId} ({ProjectName}) for stage {StageId} ({StageTitle})",
                        phase,
                        notification.UserId,
                        notification.UserEmail,
                        notification.ProjectId,
                        notification.ProjectName,
                        stage.Id,
                        stage.Title);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error creating form submission for user {UserId}, project {ProjectId}, stage {StageId} ({StageTitle})",
                        notification.UserId,
                        notification.ProjectId,
                        stage.Id,
                        stage.Title);
                    // Continue processing other stages - partial success is acceptable
                }
            }

            // Step 6: Save changes if any forms were created
            if (formsCreated > 0)
            {
                try
                {
                    await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                    logger.LogInformation(
                        "Successfully created {CreatedCount} form submission(s) for user {UserId} ({UserEmail}) in project {ProjectId} ({ProjectName}). {ExistingCount} already existed.",
                        formsCreated,
                        notification.UserId,
                        notification.UserEmail,
                        notification.ProjectId,
                        notification.ProjectName,
                        formsAlreadyExisted);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed to save form submissions for user {UserId} in project {ProjectId}. Forms will be created lazily via GetOrCreateFormSubmissionCommand.",
                        notification.UserId,
                        notification.ProjectId);
                    // Don't throw - graceful degradation
                    // GetOrCreateFormSubmissionCommand serves as fallback
                }
            }
            else if (formsAlreadyExisted > 0)
            {
                logger.LogInformation(
                    "All {Count} form submission(s) already existed for user {UserId} in project {ProjectId}. No new forms created.",
                    formsAlreadyExisted,
                    notification.UserId,
                    notification.ProjectId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error processing UserAddedToProject event for user {UserId} in project {ProjectId}. User assignment will succeed but forms may need to be created lazily.",
                notification.UserId,
                notification.ProjectId);
            // Don't throw - graceful degradation
            // User assignment should not fail because of form creation issues
        }
    }
}
