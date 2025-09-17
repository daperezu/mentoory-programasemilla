using Azure.Storage.Blobs;
using LinaSys.Shared.Application.Services;

namespace LinaSys.Web.Services;

/// <summary>
/// Service for rendering images with automatic fallback handling
/// </summary>
public class ImageRenderingService
{
    private readonly IFileStorageService? _fileStorageService;
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly ILogger<ImageRenderingService> _logger;
    private readonly IWebHostEnvironment _environment;

    public ImageRenderingService(
        IFileStorageService? fileStorageService,
        BlobServiceClient? blobServiceClient,
        ILogger<ImageRenderingService> logger,
        IWebHostEnvironment environment)
    {
        _fileStorageService = fileStorageService;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Gets an image URL with automatic fallback handling
    /// </summary>
    public async Task<string> GetImageUrlAsync(
        string? blobId,
        ImageFallbackOptions? options = null)
    {
        options ??= new ImageFallbackOptions();

        // If no blob ID provided, return fallback immediately
        if (string.IsNullOrWhiteSpace(blobId))
        {
            return GetFallbackUrl(options);
        }

        // In development, check for local file first
        if (_environment.IsDevelopment() && options.CheckLocalFirst)
        {
            var localPath = GetLocalPath(blobId);
            if (!string.IsNullOrWhiteSpace(localPath))
            {
                return localPath;
            }
        }

        // Try to get from blob storage
        if (_fileStorageService != null)
        {
            try
            {
                // If the blobId is a blob path (e.g., "public-assets/projects/tech-001/hero.jpg")
                // we need to construct the full URL
                string fullUrl;

                if (!Uri.IsWellFormedUriString(blobId, UriKind.Absolute))
                {
                    // It's a blob path, not a full URL
                    // Construct the URL using the BlobServiceClient
                    if (_blobServiceClient != null && blobId.Contains('/'))
                    {
                        // Split the path to get container and blob name
                        var firstSlashIndex = blobId.IndexOf('/');
                        var containerName = blobId.Substring(0, firstSlashIndex);
                        var blobName = blobId.Substring(firstSlashIndex + 1);

                        // Get the container client and blob client
                        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                        var blobClient = containerClient.GetBlobClient(blobName);

                        // Get the URL from the blob client
                        fullUrl = blobClient.Uri.ToString();
                    }
                    else
                    {
                        // Can't construct URL, return fallback
                        _logger.LogWarning("Cannot construct URL for blob path: {BlobId}", blobId);
                        return GetFallbackUrl(options);
                    }
                }
                else
                {
                    // It's already a full URL
                    fullUrl = blobId;
                }

                // Check if the file exists
                var exists = await _fileStorageService.FileExistsAsync(fullUrl);
                if (exists)
                {
                    // For public assets, the URL is already accessible
                    if (blobId.StartsWith("public-assets/", StringComparison.OrdinalIgnoreCase))
                    {
                        return fullUrl;
                    }

                    // For private assets, get a URL with SAS token
                    var url = await _fileStorageService.GetTemporaryAccessUrlAsync(
                        fullUrl,
                        options.UrlExpirationMinutes);

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        return url;
                    }
                }
                else
                {
                    _logger.LogDebug("Blob does not exist: {FullUrl}", fullUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get blob URL for {BlobId}", blobId);
            }
        }

        // Return fallback
        return GetFallbackUrl(options);
    }

    /// <summary>
    /// Batch get multiple image URLs efficiently
    /// </summary>
    public async Task<Dictionary<string, string>> GetImageUrlsAsync(
        IEnumerable<string?> blobIds,
        ImageFallbackOptions? options = null)
    {
        var results = new Dictionary<string, string>();
        var tasks = new List<Task>();

        foreach (var blobId in blobIds.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            tasks.Add(Task.Run(async () =>
            {
                var url = await GetImageUrlAsync(blobId, options);
                lock (results)
                {
                    results[blobId!] = url;
                }
            }));
        }

        await Task.WhenAll(tasks);
        return results;
    }

    private static string GeneratePlaceholderUrl(
        string text,
        int width,
        int height,
        string? bgColor,
        string? textColor)
    {
        var urlText = Uri.EscapeDataString(text);
        var bg = bgColor ?? "6c757d";
        var fg = textColor ?? "ffffff";

        return $"https://via.placeholder.com/{width}x{height}/{bg}/{fg}?text={urlText}";
    }

    private string GetFallbackUrl(ImageFallbackOptions options)
    {
        // If custom fallback URL provided, use it
        if (!string.IsNullOrWhiteSpace(options.CustomFallbackUrl))
        {
            return options.CustomFallbackUrl;
        }

        // If local fallback path provided, use it
        if (!string.IsNullOrWhiteSpace(options.LocalFallbackPath))
        {
            return options.LocalFallbackPath;
        }

        // Generate placeholder
        return GeneratePlaceholderUrl(
            options.PlaceholderText ?? "Image",
            options.Width,
            options.Height,
            options.BackgroundColor,
            options.TextColor);
    }

    private string? GetLocalPath(string blobId)
    {
        // Convert blob path to local path
        // e.g., "public-assets/projects/tech-001/hero.jpg" -> "/images/projects/tech-001/hero.jpg"
        if (blobId.StartsWith("public-assets/", StringComparison.OrdinalIgnoreCase))
        {
            var localPath = blobId.Replace("public-assets/", "/images/", StringComparison.OrdinalIgnoreCase);
            var fullPath = Path.Combine(_environment.WebRootPath, localPath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                return localPath;
            }
        }

        return null;
    }
}

/// <summary>
/// Options for image fallback behavior
/// </summary>
public class ImageFallbackOptions
{
    public string? CustomFallbackUrl { get; set; }
    public string? LocalFallbackPath { get; set; }
    public string? PlaceholderText { get; set; }
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 400;
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public int UrlExpirationMinutes { get; set; } = 60;
    public bool CheckLocalFirst { get; set; } = true;
}