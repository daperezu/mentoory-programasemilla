using LinaSys.Web.Models.Shared;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LinaSys.Web.TagHelpers;

[HtmlTargetElement("page-header")]
public class PageHeaderTagHelper : TagHelper
{
    [HtmlAttributeName("dropdown-items")]
    public List<PageHeaderDropdownItem>? DropdownItems { get; set; }

    [HtmlAttributeName("icon-class")]
    public string? IconClass { get; set; }

    [HtmlAttributeName("title")]
    public string Title { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var random = new Random();
        var imageIndex = random.Next(1, 10); // 1 to 9
        var imagePath = $"/assets/img/page-header/{imageIndex}.png";

        output.TagName = "div";
        output.Content.AppendHtml($@"
<div class='position-relative mb-5' style='min-height: 214px;'>
    <div class='bg-holder rounded-top' style='background-image:url({imagePath});'></div>
    <div class='position-absolute bottom-0 start-0 w-100 p-3 d-flex justify-content-between align-items-center' style='background: rgba(0,0,0,0.4);'>
        <h3 class='text-white m-0 d-flex align-items-center gap-2'>");

        if (!string.IsNullOrEmpty(IconClass))
        {
            output.Content.AppendHtml($"<i class='{IconClass}'></i>");
        }

        output.Content.AppendHtml($"{Title}</h3>");

        // Render dropdown if present
        if (DropdownItems?.Count > 0)
        {
            output.Content.AppendHtml($@"
            <div class='d-flex'>
                <button class=""btn px-3 btn-phoenix-secondary"" type=""button"" data-bs-toggle=""dropdown"" data-boundary=""window"" aria-haspopup=""true"" aria-expanded=""false"" data-bs-reference=""parent""><span class=""fa-solid fa-ellipsis""></span></button>
                <ul class=""dropdown-menu dropdown-menu-end p-0"" style=""z-index: 9999;"">");

            foreach (var item in DropdownItems)
            {
                var icon = !string.IsNullOrWhiteSpace(item.IconClass)
                    ? $"<i class='{item.IconClass} me-2'></i>"
                    : string.Empty;

                output.Content.AppendHtml($@"<li><a class=""dropdown-item"" href=""{item.Url}"">{icon}{item.Text}</a></li>");
            }

            output.Content.AppendHtml("</ul></div>");
        }

        output.Content.AppendHtml("</div></div>");
    }
}
