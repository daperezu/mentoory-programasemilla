using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LinaSys.Web.TagHelpers;

[HtmlTargetElement("span", Attributes = "asp-validation-for")]
public class ValidationMessageStyleTagHelper(IHtmlGenerator generator) : ValidationMessageTagHelper(generator)
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);

        output.AddClass("invalid-feedback", HtmlEncoder.Default);

        if (For?.Name is not null &&
            ViewContext?.ViewData?.ModelState?.TryGetValue(For.Name, out var entry) == true &&
            entry.ValidationState == ModelValidationState.Invalid)
        {
            output.AddClass("d-block", HtmlEncoder.Default);
        }
    }
}
