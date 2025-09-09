using System.Threading;
using System.Threading.Tasks;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application.IntegrationEvents.BusinessIncubator;
using LinaSys.Shared.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers
{
    /// <summary>
    /// Handles ProjectStageActivatedIntegrationEvent to send email notifications to participants.
    /// </summary>
    public class ProjectStageActivatedIntegrationEventHandler : INotificationHandler<ProjectStageActivatedIntegrationEvent>
    {
        private readonly IEmailPreferenceService _emailPreferenceService;
        private readonly IEmailQueueService _emailQueueService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IApplicationUrlService _applicationUrlService;
        private readonly ILogger<ProjectStageActivatedIntegrationEventHandler> _logger;

        public ProjectStageActivatedIntegrationEventHandler(
            IEmailPreferenceService emailPreferenceService,
            IEmailQueueService emailQueueService,
            IEmailTemplateService emailTemplateService,
            IApplicationUrlService applicationUrlService,
            ILogger<ProjectStageActivatedIntegrationEventHandler> logger)
        {
            _emailPreferenceService = emailPreferenceService;
            _emailQueueService = emailQueueService;
            _emailTemplateService = emailTemplateService;
            _applicationUrlService = applicationUrlService;
            _logger = logger;
        }

        public async Task Handle(
            ProjectStageActivatedIntegrationEvent notification,
            CancellationToken cancellationToken)
        {
            // Get dashboard URL using the application URL service
            var dashboardUrl = _applicationUrlService.GetParticipantProjectDashboardUrl(notification.ProjectId);

            // Create notification for each participant
            foreach (var participant in notification.Participants)
            {
                // Check if user wants these notifications
                var canSend = await _emailPreferenceService.CanSendEmailAsync(participant.UserId, "email.system.stageActivated", cancellationToken);
                if (!canSend)
                {
                    _logger.LogInformation("Skipping stage activation notification for user {UserId} due to preferences", participant.UserId);
                    continue;
                }

                // Generate email using template service
                var emailBody = _emailTemplateService.GenerateProjectStageActivatedEmail(
                    participant.FullName,
                    notification.ProjectName,
                    notification.StageName,
                    GetStageTypeDisplayName(notification.StageType),
                    notification.StartDate,
                    notification.EndDate,
                    dashboardUrl);

                // Queue email
                _emailQueueService.QueueEmail(
                    participant.Email,
                    $"🚀 Nueva etapa activada en {notification.ProjectName}",
                    emailBody);

                _logger.LogInformation("Queued stage activation notification for project {ProjectId} to user {UserId} ({Email})",
                    notification.ProjectId, participant.UserId, participant.Email);
            }

            await Task.CompletedTask;
        }

        private static string GetStageTypeDisplayName(string stageType)
        {
            return stageType switch
            {
                "InitialFormCollection" => "Formulario Inicial",
                "FinalFormCollection" => "Formulario Final",
                _ => stageType
            };
        }
    }
}