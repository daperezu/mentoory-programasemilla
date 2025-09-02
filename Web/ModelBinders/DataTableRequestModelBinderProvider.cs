using LinaSys.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public class DataTableRequestModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(DataTableRequest) ? new BinderTypeModelBinder(typeof(DataTableRequestModelBinder)) : null;
    }
}
