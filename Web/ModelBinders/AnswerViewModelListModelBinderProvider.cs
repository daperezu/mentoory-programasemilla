using LinaSys.Web.Areas.Diagnostics.Models.DiagnosisForms;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LinaSys.Web.ModelBinders;

public class AnswerViewModelListModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) => context.Metadata.ModelType == typeof(IEnumerable<AnswerViewModel>) ? new AnswerViewModelListModelBinder() : null;
}
