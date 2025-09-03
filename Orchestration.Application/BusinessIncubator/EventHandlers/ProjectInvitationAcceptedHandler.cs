using LinaSys.BusinessIncubator.Application.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.BusinessIncubator.EventHandlers;

/// <summary>
/// Handles the ProjectInvitationAccepted integration event to create initial tasks for participants.
/// </summary>
public sealed class ProjectInvitationAcceptedHandler(
    IBusinessIncubatorRepository businessIncubatorRepository,
    IStarterRepository starterRepository,
    ITimeProvider timeProvider,
    ILogger<ProjectInvitationAcceptedHandler> logger) : INotificationHandler<ProjectInvitationAccepted>
{
    public async Task Handle(ProjectInvitationAccepted notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing ProjectInvitationAccepted for user {UserId} in project {ProjectId}",
                notification.UserId,
                notification.ProjectId);

            // Get project with stages
            var project = await businessIncubatorRepository.GetProjectWithStagesByExternalIdAsync(
                notification.ProjectExternalId,
                cancellationToken);

            if (project is null)
            {
                logger.LogWarning(
                    "Project {ProjectId} not found for invitation acceptance",
                    notification.ProjectId);
                return;
            }

            // Determine current stage
            var currentDate = timeProvider.UtcNow;
            var currentStage = project.GetCurrentStage(currentDate);

            // Create appropriate task based on stage
            StarterTask task;
            if (currentStage is not null)
            {
                var taskTitle = currentStage.Type switch
                {
                    ProjectStageType.InitialFormCollection => "Completar Formulario Inicial",
                    ProjectStageType.FinalFormCollection => "Completar Formulario Final",
                    _ => "Bienvenido al Programa"
                };

                var taskDescription = currentStage.Type switch
                {
                    ProjectStageType.InitialFormCollection =>
                        "Por favor complete el formulario inicial de diagnóstico. Este formulario nos ayudará a entender mejor su proyecto y necesidades.",
                    ProjectStageType.FinalFormCollection =>
                        "Por favor complete el formulario final de evaluación. Este formulario nos ayudará a medir el progreso de su proyecto.",
                    _ =>
                        $"Bienvenido al programa {project.Name}. Revise los recursos disponibles y espere instrucciones del equipo coordinador."
                };

                var taskType = currentStage.Type switch
                {
                    ProjectStageType.InitialFormCollection => "form_initial",
                    ProjectStageType.FinalFormCollection => "form_final",
                    _ => "welcome"
                };

                task = new StarterTask(
                    projectId: notification.ProjectId,
                    assignedToUserId: notification.UserId,
                    title: taskTitle,
                    description: taskDescription,
                    createdDate: timeProvider.UtcNow,
                    type: taskType,
                    priority: "high",
                    dueDate: currentStage.EndDate,
                    assignedBy: "Sistema",
                    category: "onboarding");
            }
            else
            {
                // No active stage, create a welcome task
                task = new StarterTask(
                    projectId: notification.ProjectId,
                    assignedToUserId: notification.UserId,
                    title: "Bienvenido al Programa",
                    description: $"Bienvenido al programa {project.Name}. Pronto recibirá más información sobre los siguientes pasos.",
                    createdDate: timeProvider.UtcNow,
                    type: "welcome",
                    priority: "normal",
                    dueDate: null,
                    assignedBy: "Sistema",
                    category: "onboarding");
            }

            // Save task to repository
            await starterRepository.AddTaskAsync(task);
            await starterRepository.UnitOfWork.SaveEntitiesAsync();

            logger.LogInformation(
                "Created task '{TaskTitle}' for user {UserId} in project {ProjectId}",
                task.Title,
                notification.UserId,
                notification.ProjectId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing ProjectInvitationAccepted for user {UserId} in project {ProjectId}",
                notification.UserId,
                notification.ProjectId);
            throw;
        }
    }
}