using System.Text.Json;
using LinaSys.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinaSys.Web.Attributes;

public static class RestoreModelAndState
{
    public const string TempModelKey = "__TempModel__";
    public const string TempModelStateKey = "__TempModelState__";
}

[AttributeUsage(AttributeTargets.Method)]
public class RestoreModelAndStateAttribute<T> : ActionFilterAttribute
    where T : RestorableViewModel
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is not Controller controller)
        {
            return;
        }

        if (!controller.TempData.TryGetValue(RestoreModelAndState.TempModelKey, out var rawModel) || rawModel is not string jsonModel)
        {
            return;
        }

        var parameterName = context.ActionDescriptor.Parameters.FirstOrDefault(p => p.ParameterType == typeof(T))?.Name;

        if (parameterName is null)
        {
            return;
        }

        var restoredModel = JsonSerializer.Deserialize<T>(jsonModel);
        if (restoredModel is null)
        {
            return;
        }

        restoredModel.WasRestored = true;
        context.ActionArguments[parameterName] = restoredModel;

        controller.ModelState.Clear();
        controller.TryValidateModel(restoredModel);

        if (controller.TempData.TryGetValue(RestoreModelAndState.TempModelStateKey, out var rawErrors) && rawErrors is string jsonErrors)
        {
            var errors = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonErrors);
            if (errors is not null)
            {
                foreach (var (key, messages) in errors)
                {
                    foreach (var msg in messages)
                    {
                        controller.ModelState.TryAddModelError(key, msg);
                    }
                }
            }
        }
    }
}
