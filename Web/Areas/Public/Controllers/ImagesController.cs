using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace LinaSys.Web.Areas.Public.Controllers;

/// <summary>
/// High-performance image handler with streaming and automatic fallback.
/// </summary>
[Area("Public")]
[Route("[area]/[controller]")]
[AllowAnonymous]
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
public class ImagesController : Controller
{
    private readonly IFileStorageService? _fileStorageService;
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly ILogger<ImagesController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ImagesController(
        IFileStorageService? fileStorageService,
        BlobServiceClient? blobServiceClient,
        ILogger<ImagesController> logger,
        IWebHostEnvironment environment)
    {
        _fileStorageService = fileStorageService;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Streams an image by its blob ID with automatic fallback.
    /// </summary>
    [HttpGet("{*encodedBlobId}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept")]
    public async Task<IActionResult> Get(
        string encodedBlobId,
        [FromQuery] string? type = "default",
        [FromQuery] int? width = null,
        [FromQuery] int? height = null,
        [FromQuery] string? text = null)
    {
        try
        {
            // Decode the blob ID
            var blobId = Uri.UnescapeDataString(encodedBlobId);

            // Generate ETag for caching
            var etag = GenerateETag(blobId, type, width, height, text);

            // Check if client has cached version
            if (HttpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var incomingEtag))
            {
                if (string.Equals(incomingEtag, etag, StringComparison.Ordinal))
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
            }

            // Add ETag to response
            HttpContext.Response.Headers.Append(HeaderNames.ETag, etag);

            // Vary by query string parameters manually
            HttpContext.Response.Headers.Append(HeaderNames.Vary, "Accept");

            // Try to stream from local files in development
            if (_environment.IsDevelopment())
            {
                var localResult = TryStreamLocalImage(blobId);
                if (localResult != null)
                {
                    return localResult;
                }
            }

            // Try to stream from blob storage
            if (!string.IsNullOrWhiteSpace(blobId) && blobId != "placeholder")
            {
                var blobResult = await TryStreamBlobImage(blobId);
                if (blobResult != null)
                {
                    return blobResult;
                }
            }

            // Stream SVG placeholder
            return StreamSvgPlaceholder(
                width ?? GetDefaultWidth(type),
                height ?? GetDefaultHeight(type),
                text ?? GetDefaultText(type));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving image: {EncodedBlobId}", encodedBlobId);

            // Return error placeholder
            return StreamSvgPlaceholder(400, 300, "Error");
        }
    }

    private static string GenerateETag(string blobId, string? type, int? width, int? height, string? text)
    {
        var input = $"{blobId}|{type}|{width}|{height}|{text}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }

    private static int GetDefaultWidth(string? type) => type?.ToLowerInvariant() switch
    {
        "hero" => 800,
        "thumbnail" => 300,
        "avatar" => 150,
        _ => 400
    };

    private static int GetDefaultHeight(string? type) => type?.ToLowerInvariant() switch
    {
        "hero" => 400,
        "thumbnail" => 200,
        "avatar" => 150,
        _ => 300
    };

    private static string GetDefaultText(string? type) => type?.ToLowerInvariant() switch
    {
        "hero" => "Proyecto",
        "avatar" => "Usuario",
        _ => "Imagen"
    };

    private static string GetContentType(string blobId)
    {
        var extension = Path.GetExtension(blobId)?.ToLowerInvariant() ?? string.Empty;
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }

    private IActionResult? TryStreamLocalImage(string blobId)
    {
        try
        {
            // Convert blob path to local path
            if (blobId.StartsWith("public-assets/", StringComparison.OrdinalIgnoreCase))
            {
                var localPath = blobId.Replace("public-assets/", "images/", StringComparison.OrdinalIgnoreCase);
                var fullPath = Path.Combine(_environment.WebRootPath, localPath.Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(fullPath))
                {
                    // Stream the file directly without loading into memory
                    var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

                    // FileStreamResult will dispose the stream automatically
                    return new FileStreamResult(fileStream, GetContentType(blobId))
                    {
                        EnableRangeProcessing = true, // Support partial content requests
                        LastModified = System.IO.File.GetLastWriteTimeUtc(fullPath)
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not stream local image: {BlobId}", blobId);
        }

        return null;
    }

    private async Task<IActionResult?> TryStreamBlobImage(string blobId)
    {
        if (_blobServiceClient == null)
        {
            return null;
        }

        // Use aggressive timeout to fail fast if storage is down
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        try
        {
            // Parse container and blob name from the path
            if (blobId.Contains('/'))
            {
                var firstSlashIndex = blobId.IndexOf('/');
                var containerName = blobId.Substring(0, firstSlashIndex);
                var blobName = blobId.Substring(firstSlashIndex + 1);

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Check if blob exists and get properties with timeout
                try
                {
                    // Fail fast with cancellation token
                    var properties = await blobClient.GetPropertiesAsync(
                        conditions: null,
                        cancellationToken: cts.Token);

                    // Stream the blob directly to the response with timeout
                    var downloadResponse = await blobClient.OpenReadAsync(new BlobOpenReadOptions(allowModifications: false)
                    {
                        BufferSize = 4096, // 4KB buffer for streaming
                        Conditions = new BlobRequestConditions
                        {
                            IfModifiedSince = Request.Headers.IfModifiedSince.FirstOrDefault() != null
                                ? DateTimeOffset.Parse(Request.Headers.IfModifiedSince.FirstOrDefault()!)
                                : null
                        }
                    }, cancellationToken: cts.Token);

                    // Return the stream result
                    return new FileStreamResult(downloadResponse, properties.Value.ContentType ?? GetContentType(blobId))
                    {
                        EnableRangeProcessing = true,
                        LastModified = properties.Value.LastModified,
                        EntityTag = new EntityTagHeaderValue($"\"{properties.Value.ETag}\"")
                    };
                }
                catch (OperationCanceledException)
                {
                    // Timeout - storage is likely down
                    _logger.LogWarning("Blob storage timeout for {BlobId} - storage may be down", blobId);
                    return null;
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    _logger.LogDebug("Blob not found: {BlobId}", blobId);
                    return null;
                }
                catch (RequestFailedException ex) when (ex.Status == 304)
                {
                    // Not modified
                    return StatusCode(StatusCodes.Status304NotModified);
                }
                catch (RequestFailedException ex) when (ex.ErrorCode == "ContainerNotFound")
                {
                    _logger.LogDebug("Container not found for blob: {BlobId}", blobId);
                    return null;
                }
                catch (RequestFailedException ex) when (ex.Message.Contains("Connection refused") ||
                                                         ex.Message.Contains("No such host") ||
                                                         ex.Message.Contains("actively refused"))
                {
                    // Storage service is down
                    _logger.LogWarning("Blob storage appears to be down: {Message}", ex.Message);
                    return null;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout - storage is likely down
            _logger.LogWarning("Blob storage timeout for {BlobId} - storage may be down", blobId);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not stream blob image: {BlobId}", blobId);
        }

        return null;
    }

    private IActionResult StreamSvgPlaceholder(int width, int height, string text)
    {
        // Generate SVG and stream it directly
        var svg = $@"<svg width=""{width}"" height=""{height}"" xmlns=""http://www.w3.org/2000/svg"">
    <rect width=""100%"" height=""100%"" fill=""#6c757d""/>
    <text x=""50%"" y=""50%""
          font-family=""Arial, sans-serif""
          font-size=""{Math.Min(width, height) / 10}""
          fill=""white""
          text-anchor=""middle""
          dy="".3em"">
        {System.Security.SecurityElement.Escape(text ?? string.Empty)}
    </text>
</svg>";

        // Stream SVG directly to response
        var svgBytes = Encoding.UTF8.GetBytes(svg);
        var stream = new MemoryStream(svgBytes);

        return new FileStreamResult(stream, "image/svg+xml")
        {
            EnableRangeProcessing = false // SVG is small, no need for range support
        };
    }
}