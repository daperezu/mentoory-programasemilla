using LinaSys.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class DataTableRequestModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var form = bindingContext.HttpContext.Request.Form;

        var columnIndex = form["order[0][column]"];

        var request = new DataTableRequest
        {
            Draw = int.Parse(form["draw"]!),
            Start = int.Parse(form["start"]!),
            Length = int.Parse(form["length"]!),
            GlobalSearch = form["search[value]"],
            OrderDirection = form["order[0][dir]"],
            OrderByColumn = form[$"columns[{columnIndex}][data]"],
        };

        int i = 0;
        while (form.ContainsKey($"columns[{i}][data]"))
        {
            var column = form[$"columns[{i}][data]"].ToString();
            var columnSearch = form[$"columns[{i}][search][value]"].ToString();

            request.ColumnSearches[column] = string.IsNullOrWhiteSpace(columnSearch) ? null : columnSearch;
            i++;
        }

        bindingContext.Result = ModelBindingResult.Success(request);
        return Task.CompletedTask;
    }
}
