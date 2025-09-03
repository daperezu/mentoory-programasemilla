using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Dashboard.Commands.UpdateWidgetLayout;

public class UpdateWidgetLayoutCommand(string userId, List<WidgetLayoutItem> widgetLayouts) : IBaseRequest
{
    public string UserId { get; } = userId;
    public List<WidgetLayoutItem> WidgetLayouts { get; } = widgetLayouts;
}

public class WidgetLayoutItem
{
    public long WidgetId { get; set; }
    public int Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsVisible { get; set; }
    public string? Configuration { get; set; }
}