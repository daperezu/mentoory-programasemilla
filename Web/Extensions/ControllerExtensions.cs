using System.Text.Json;
using LinaSys.Web.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Extensions;

public static class ControllerExtensions
{
    private const string ToastKey = "ToastMessage";

    public static void SetErrorToast(this Controller controller, string message)
    {
        controller.SetToast(new ToastMessage(message, "danger"));
    }

    public static void SetInfoToast(this Controller controller, string message)
    {
        controller.SetToast(new ToastMessage(message, "info"));
    }

    public static void SetSuccessToast(this Controller controller, string message)
    {
        controller.SetToast(new ToastMessage(message, "success"));
    }

    public static void SetWarnToast(this Controller controller, string message)
    {
        controller.SetToast(new ToastMessage(message, "warning"));
    }

    private static void SetToast(this Controller controller, ToastMessage toast)
    {
        controller.TempData[ToastKey] = JsonSerializer.Serialize(toast, JsonSerializerOptions.Web);
    }
}
