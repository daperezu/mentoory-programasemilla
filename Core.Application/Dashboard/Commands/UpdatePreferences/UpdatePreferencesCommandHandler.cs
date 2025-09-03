using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Core.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Dashboard.Commands.UpdatePreferences;

public class UpdatePreferencesCommandHandler(
    IDashboardRepository dashboardRepository,
    IPreferencesRepository preferencesRepository) : BaseCommandHandler<UpdatePreferencesCommand>
{
    public override async Task<Result> Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user's dashboard
            var dashboard = await dashboardRepository.GetByUserIdAsync(request.UserId);
            if (dashboard is null)
            {
                return Failure(ResultErrorCodes.GenericError, ("UserId", "Dashboard no encontrado para el usuario"));
            }

            // Create new preferences
            var preferences = new DashboardPreferences(
                request.Theme,
                request.Language,
                request.RefreshInterval,
                request.ShowNotifications,
                request.PlayNotificationSound,
                request.ShowTaskReminders,
                request.AutoRefreshEnabled,
                request.CompactView,
                request.ShowWidgetHeaders,
                request.EnableAnimations,
                request.DateFormat,
                request.TimeFormat,
                request.Timezone);

            // Update dashboard preferences
            dashboard.UpdatePreferences(preferences);

            // Save changes
            await dashboardRepository.UpdateAsync(dashboard);

            // Also save to preferences table for quick access
            await preferencesRepository.SaveUserPreferencesAsync(request.UserId, preferences);

            // Save all changes via UnitOfWork
            await preferencesRepository.UnitOfWork.SaveEntitiesAsync();

            return Success();
        }
        catch (Exception ex)
        {
            return Failure(ResultErrorCodes.Unknown, ("Error", $"Error al actualizar preferencias: {ex.Message}"));
        }
    }
}