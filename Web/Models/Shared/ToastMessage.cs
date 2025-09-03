namespace LinaSys.Web.Models.Shared;

public record ToastMessage(string Message, string Type = "info", string? Icon = null)
{
    public static ToastMessage Error(string message, string? icon = null) => new(message, "danger", icon);

    public static ToastMessage Info(string message, string? icon = null) => new(message, "info", icon);

    public static ToastMessage Success(string message, string? icon = null) => new(message, "success", icon);

    public static ToastMessage Warning(string message, string? icon = null) => new(message, "warning", icon);
}
