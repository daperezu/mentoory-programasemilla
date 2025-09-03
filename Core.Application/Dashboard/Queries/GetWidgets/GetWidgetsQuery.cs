using MediatR;

namespace LinaSys.Core.Application.Dashboard.Queries.GetWidgets;

public class GetWidgetsQuery(string userId, string role, bool onlyVisible = true) : IRequest<List<WidgetDto>>
{
    public string UserId { get; } = userId;
    public string Role { get; } = role;
    public bool OnlyVisible { get; } = onlyVisible;
}

public class WidgetDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public int Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsVisible { get; set; }
    public bool IsResizable { get; set; }
    public bool IsDraggable { get; set; }
    public bool IsRefreshable { get; set; }
    public string? Configuration { get; set; }
    public string? ApiEndpoint { get; set; }
    public int? RefreshInterval { get; set; }
    public object? Data { get; set; }
}
