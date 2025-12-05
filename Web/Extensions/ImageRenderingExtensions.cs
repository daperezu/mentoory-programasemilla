using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Extensions;

/// <summary>
/// Generic image rendering extensions for any type of image with automatic fallbacks.
/// </summary>
public static class ImageRenderingExtensions
{
    /// <summary>
    /// Renders an image with automatic fallback to placeholder if blob doesn't exist
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<IHtmlContent> RenderImageAsync(
        this IHtmlHelper htmlHelper,
        IFileStorageService? fileStorageService,
        string? blobId,
        string altText,
        string cssClass = "img-fluid",
        string? fallbackUrl = null,
        int width = 800,
        int height = 400)
    {
        var imageUrl = await GetImageUrlWithFallbackAsync(
            fileStorageService,
            blobId,
            fallbackUrl ?? GeneratePlaceholder(altText, width, height));

        var img = new TagBuilder("img");
        img.Attributes["src"] = imageUrl;
        img.Attributes["alt"] = altText;
        img.Attributes["class"] = cssClass;
        img.Attributes["loading"] = "lazy"; // Add lazy loading by default

        return img;
    }

    /// <summary>
    /// Gets an image URL with automatic fallback handling
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<string> GetImageUrlWithFallbackAsync(
        this IFileStorageService? fileStorageService,
        string? blobId,
        string? fallbackUrl = null)
    {
        // If no blob ID provided, return fallback immediately
        if (string.IsNullOrWhiteSpace(blobId))
        {
            return fallbackUrl ?? GeneratePlaceholder("Image", 800, 400);
        }

        // If we have storage service, try to get the actual blob URL
        if (fileStorageService != null)
        {
            try
            {
                // Check if the file exists
                var exists = await fileStorageService.FileExistsAsync(blobId);
                if (exists)
                {
                    // For public-assets container, we might not need SAS token
                    // but let's get a URL anyway
                    var url = await fileStorageService.GetTemporaryAccessUrlAsync(blobId, 60);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        return url;
                    }
                }
            }
            catch (Exception)
            {
                // Log error if needed, fall through to placeholder
            }
        }

        // Return fallback URL
        return fallbackUrl ?? GeneratePlaceholder("Image", 800, 400);
    }

    /// <summary>
    /// Generates a placeholder image URL with specified text and dimensions
    /// </summary>
    /// <returns></returns>
    public static string GeneratePlaceholder(string text, int width = 800, int height = 400, string? bgColor = null, string? textColor = null)
    {
        // Clean up text for URL
        var urlText = Uri.EscapeDataString(text.Replace(" ", "+"));
        var bg = bgColor ?? "6c757d"; // Bootstrap secondary color
        var fg = textColor ?? "ffffff";

        return $"https://via.placeholder.com/{width}x{height}/{bg}/{fg}?text={urlText}";
    }

    /// <summary>
    /// Generates a category-based placeholder with automatic color selection
    /// </summary>
    /// <returns></returns>
    public static string GenerateCategoryPlaceholder(string category, string? text = null, int width = 800, int height = 400)
    {
        var colors = GetCategoryColors(category);
        var displayText = text ?? GetCategoryDisplayText(category);
        return GeneratePlaceholder(displayText, width, height, colors.Bg, colors.Fg);
    }

    /// <summary>
    /// Creates a responsive picture element with multiple sources
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<IHtmlContent> RenderResponsiveImageAsync(
        this IHtmlHelper htmlHelper,
        IFileStorageService? fileStorageService,
        string? blobId,
        string altText,
        string cssClass = "img-fluid",
        Dictionary<string, int>? breakpoints = null)
    {
        var imageUrl = await GetImageUrlWithFallbackAsync(fileStorageService, blobId);

        var picture = new TagBuilder("picture");

        // Add responsive sources if breakpoints provided
        if (breakpoints != null)
        {
            foreach (var bp in breakpoints.OrderByDescending(x => x.Value))
            {
                var source = new TagBuilder("source");
                source.Attributes["media"] = $"(min-width: {bp.Value}px)";
                source.Attributes["srcset"] = imageUrl; // In production, could generate different sizes
                source.TagRenderMode = TagRenderMode.SelfClosing;
                picture.InnerHtml.AppendHtml(source);
            }
        }

        // Add the default img tag
        var img = new TagBuilder("img");
        img.Attributes["src"] = imageUrl;
        img.Attributes["alt"] = altText;
        img.Attributes["class"] = cssClass;
        img.Attributes["loading"] = "lazy";

        picture.InnerHtml.AppendHtml(img);

        return picture;
    }

    /// <summary>
    /// Renders an avatar image with fallback to initials
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<IHtmlContent> RenderAvatarAsync(
        this IHtmlHelper htmlHelper,
        IFileStorageService? fileStorageService,
        string? avatarBlobId,
        string userName,
        int size = 40,
        string cssClass = "rounded-circle")
    {
        if (!string.IsNullOrWhiteSpace(avatarBlobId) && fileStorageService != null)
        {
            var exists = await fileStorageService.FileExistsAsync(avatarBlobId);
            if (exists)
            {
                var url = await fileStorageService.GetTemporaryAccessUrlAsync(avatarBlobId, 60);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var img = new TagBuilder("img");
                    img.Attributes["src"] = url;
                    img.Attributes["alt"] = userName;
                    img.Attributes["class"] = cssClass;
                    img.Attributes["width"] = size.ToString();
                    img.Attributes["height"] = size.ToString();
                    return img;
                }
            }
        }

        // Fallback to initials
        var initials = GetInitials(userName);
        var div = new TagBuilder("div");
        div.Attributes["class"] = $"{cssClass} d-flex align-items-center justify-content-center bg-primary text-white";
        div.Attributes["style"] = $"width: {size}px; height: {size}px; font-size: {size / 2}px;";
        div.InnerHtml.Append(initials);

        return div;
    }

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        if (parts.Length == 1)
        {
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        }

        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    private static (string Bg, string Fg) GetCategoryColors(string category)
    {
        return category.ToUpperInvariant() switch
        {
            "TECH" or "TECHNOLOGY" => ("0066CC", "FFFFFF"),
            "BIO" or "BIOTECHNOLOGY" => ("00AA44", "FFFFFF"),
            "EDU" or "EDUCATION" => ("FF6600", "FFFFFF"),
            "AGRO" or "AGRICULTURE" => ("228B22", "FFFFFF"),
            "LOG" or "LOGISTICS" => ("FF9900", "FFFFFF"),
            "FIN" or "FINANCE" => ("003366", "FFFFFF"),
            "GREEN" or "SUSTAINABILITY" => ("00AA00", "FFFFFF"),
            "HEALTH" or "MEDICAL" => ("FF0066", "FFFFFF"),
            "TOUR" or "TOURISM" => ("0099CC", "FFFFFF"),
            "SOCIAL" => ("9933CC", "FFFFFF"),
            "INNOV" or "INNOVATION" => ("6C63FF", "FFFFFF"),
            _ => ("6c757d", "FFFFFF") // Bootstrap secondary
        };
    }

    private static string GetCategoryDisplayText(string category)
    {
        return category.ToUpperInvariant() switch
        {
            "TECH" => "Tecnología",
            "BIO" => "Biotecnología",
            "EDU" => "Educación",
            "AGRO" => "Agricultura",
            "LOG" => "Logística",
            "FIN" => "Finanzas",
            "GREEN" => "Sostenibilidad",
            "HEALTH" => "Salud",
            "TOUR" => "Turismo",
            "SOCIAL" => "Impacto Social",
            "INNOV" => "Innovación",
            _ => "Imagen"
        };
    }
}