using System.Text.Json;
using LinaSys.Core.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Dashboard.Commands.UpdateWidgetLayout;

public class UpdateWidgetLayoutCommandHandler(IDashboardRepository dashboardRepository) : BaseCommandHandler<UpdateWidgetLayoutCommand>
{
    public override async Task<Result> Handle(UpdateWidgetLayoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user's dashboard
            var dashboard = await dashboardRepository.GetByUserIdAsync(request.UserId);
            if (dashboard is null)
            {
                return Failure(ResultErrorCodes.GenericError, ("UserId", "Dashboard no encontrado para el usuario"));
            }

            // Update widget layouts
            // Convert the widget layouts to JSON and update the dashboard layout
            if (request.WidgetLayouts?.Any() == true)
            {
                var layoutJson = JsonSerializer.Serialize(request.WidgetLayouts);
                dashboard.UpdateLayout(layoutJson);

                // Also update individual widget configurations if needed
                foreach (var widgetLayout in request.WidgetLayouts.Where(w => !string.IsNullOrEmpty(w.Configuration)))
                {
                    dashboard.UpdateWidgetConfiguration(widgetLayout.WidgetId, widgetLayout.Configuration!);
                }
            }

            // Save changes
            await dashboardRepository.UpdateAsync(dashboard);
            await dashboardRepository.SaveChangesAsync();

            return Success();
        }
        catch (Exception ex)
        {
            return Failure(ResultErrorCodes.Unknown, ("Error", $"Error al actualizar el diseño de widgets: {ex.Message}"));
        }
    }
}