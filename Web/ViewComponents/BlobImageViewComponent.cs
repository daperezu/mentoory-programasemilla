using Microsoft.AspNetCore.Mvc;
using LinaSys.Web.Services;

namespace LinaSys.Web.ViewComponents;

/// <summary>
/// ViewComponent for rendering blob images with automatic fallback
/// </summary>
public class BlobImageViewComponent : ViewComponent
{
    private readonly ImageRenderingService _imageRenderingService;

    public BlobImageViewComponent(ImageRenderingService imageRenderingService)
    {
        _imageRenderingService = imageRenderingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        string? blobId,
        string altText = "Image",
        string cssClass = "img-fluid",
        int width = 800,
        int height = 400,
        string? fallbackUrl = null,
        string? placeholderText = null)
    {
        var options = new ImageFallbackOptions
        {
            Width = width,
            Height = height,
            CustomFallbackUrl = fallbackUrl,
            PlaceholderText = placeholderText ?? altText
        };

        var imageUrl = await _imageRenderingService.GetImageUrlAsync(blobId, options);

        var model = new BlobImageViewModel
        {
            ImageUrl = imageUrl,
            AltText = altText,
            CssClass = cssClass,
            Width = width,
            Height = height
        };

        return View(model);
    }
}

public class BlobImageViewModel
{
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}