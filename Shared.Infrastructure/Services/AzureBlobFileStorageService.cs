using System.Security.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinaSys.Shared.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage implementation of file storage service with structured naming and metadata.
/// </summary>
public class AzureBlobFileStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<StorageSettings> storageSettings,
    ILogger<AzureBlobFileStorageService> logger) : IFileStorageService
{
    private static readonly Dictionary<FileCategory, string> _containerMapping = new()
    {
        { FileCategory.Avatar, "avatars" },
        { FileCategory.Document, "documents" },
        { FileCategory.Report, "reports" },
        { FileCategory.Diagnostic, "diagnostics" },
        { FileCategory.Temporary, "temporary" },
        { FileCategory.PublicAsset, "public-assets" }
    };

    private readonly BlobServiceClient _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
    private readonly ILogger<AzureBlobFileStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly StorageSettings _storageSettings = storageSettings?.Value ?? new StorageSettings();

    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return false;
            }

            var (containerName, blobName) = ExtractContainerAndBlobName(fileUrl);
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
            {
                _logger.LogWarning("Could not extract container and blob name from URL: {FileUrl}", fileUrl);
                return false;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots,
                cancellationToken: cancellationToken);

            if (response.Value)
            {
                _logger.LogInformation("File deleted successfully: {FileUrl}", fileUrl);
            }

            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobName) = ExtractContainerAndBlobName(fileUrl);
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
            {
                return null;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response?.Value?.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file: {FileUrl}", fileUrl);
            return null;
        }
    }

    public async Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobName) = ExtractContainerAndBlobName(fileUrl);
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
            {
                return false;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.ExistsAsync(cancellationToken);
            return response?.Value ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check file existence: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<FileMetadata?> GetFileMetadataAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobName) = ExtractContainerAndBlobName(fileUrl);
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
            {
                return null;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            if (response == null)
            {
                return null;
            }

            var properties = response.Value;

            // Get tags
            var tagsResponse = await blobClient.GetTagsAsync(cancellationToken: cancellationToken);
            var tags = tagsResponse?.Value?.Tags ?? new Dictionary<string, string>();

            return new FileMetadata
            {
                Url = fileUrl,
                FileName = properties.Metadata.TryGetValue("originalFileName", out var origName) ? origName : Path.GetFileName(blobName),
                ContentType = properties.ContentType,
                SizeInBytes = properties.ContentLength,
                ContentMD5 = properties.ContentHash != null ? Convert.ToBase64String(properties.ContentHash) : null,
                Category = DetermineCategoryFromContainer(containerName),
                Domain = tags.TryGetValue("domain", out var domain) ? domain : string.Empty,
                EntityType = tags.TryGetValue("entityType", out var entityType) ? entityType : string.Empty,
                EntityId = tags.TryGetValue("entityId", out var entityId) ? entityId : string.Empty,
                ContainsPII = bool.Parse(tags.TryGetValue("pii", out var pii) ? pii : "false"),
                UploadedAt = properties.CreatedOn.DateTime,
                UploadedBy = properties.Metadata.TryGetValue("uploadedBy", out var uploadedBy) ? uploadedBy : "system",
                Tags = new Dictionary<string, string>(tags),
                CustomMetadata = new Dictionary<string, string>(properties.Metadata)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get metadata for: {FileUrl}", fileUrl);
            return null;
        }
    }

    public Task<string> GetTemporaryAccessUrlAsync(
        string fileUrl,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return Task.FromResult(string.Empty);
            }

            var (containerName, blobName) = ExtractContainerAndBlobName(fileUrl);
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
            {
                return Task.FromResult(fileUrl);
            }

            // Public assets don't need SAS tokens
            if (containerName == _containerMapping[FileCategory.PublicAsset])
            {
                return Task.FromResult(fileUrl);
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!blobClient.CanGenerateSasUri)
            {
                _logger.LogWarning("Cannot generate SAS token for blob: {BlobName}", blobName);
                return Task.FromResult(fileUrl);
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return Task.FromResult(sasUri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate temporary access URL for: {FileUrl}", fileUrl);
            return Task.FromResult(fileUrl);
        }
    }

    public async Task<IReadOnlyList<FileMetadata>> ListFilesByEntityAsync(
        string domain,
        string entityType,
        string entityId,
        FileCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<FileMetadata>();

        try
        {
            var containers = category.HasValue
                ? [GetContainerName(category.Value)]
                : _containerMapping.Values.ToArray();

            foreach (var containerName in containers)
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Search by tags
                var tagFilter = $"\"domain\"='{domain}' AND \"entityType\"='{entityType}' AND \"entityId\"='{entityId}'";

                await foreach (var taggedBlob in containerClient.FindBlobsByTagsAsync(tagFilter, cancellationToken))
                {
                    var blobClient = containerClient.GetBlobClient(taggedBlob.BlobName);
                    var metadata = await GetFileMetadataAsync(blobClient.Uri.ToString(), cancellationToken);
                    if (metadata != null)
                    {
                        results.Add(metadata);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list files for entity: {Domain}/{EntityType}/{EntityId}", domain, entityType, entityId);
        }

        return results;
    }

    public async Task<FileMetadata> UploadFileAsync(
                                Stream fileStream,
        string fileName,
        string contentType,
        FileUploadOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateUploadOptions(options);
            ValidateFileSize(fileStream, options.Category);

            var containerName = GetContainerName(options.Category);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure container exists with appropriate access level
            var publicAccess = options.Category == FileCategory.PublicAsset
                ? PublicAccessType.Blob
                : PublicAccessType.None;

            await containerClient.CreateIfNotExistsAsync(
                publicAccessType: publicAccess,
                cancellationToken: cancellationToken);

            // Generate structured blob name
            var blobName = GenerateBlobName(fileName, options);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Delete existing if overwrite is enabled
            if (options.OverwriteIfExists)
            {
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }

            // Calculate MD5 if enabled
            string? contentMD5 = null;
            if (_storageSettings.EnableMD5Validation)
            {
                contentMD5 = await CalculateMD5Async(fileStream);
                fileStream.Position = 0; // Reset stream position
            }

            // Prepare blob upload options
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                ContentHash = contentMD5 != null ? Convert.FromBase64String(contentMD5) : null,
                CacheControl = GetCacheControl(options.Category)
            };

            // Prepare tags
            var tags = PrepareBlobTags(options);

            // Prepare metadata
            var metadata = PrepareMetadata(options, fileName);

            // Upload blob
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Tags = tags,
                Metadata = metadata
            };

            var response = await blobClient.UploadAsync(
                fileStream,
                uploadOptions,
                cancellationToken);

            var fileMetadata = new FileMetadata
            {
                FileId = Guid.NewGuid().ToString(),
                Url = blobClient.Uri.ToString(),
                FileName = fileName,
                ContentType = contentType,
                SizeInBytes = fileStream.Length,
                ContentMD5 = contentMD5,
                Category = options.Category,
                Domain = options.Domain,
                EntityType = options.EntityType,
                EntityId = options.EntityId,
                ContainsPII = options.ContainsPII,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = metadata.TryGetValue("uploadedBy", out var uploadedBy) ? uploadedBy : "system",
                Tags = tags,
                CustomMetadata = options.CustomMetadata ?? new Dictionary<string, string>()
            };

            _logger.LogInformation(
                "File uploaded successfully: {Category}/{Domain}/{EntityType}/{EntityId} - {FileName}",
                options.Category,
                options.Domain,
                options.EntityType,
                options.EntityId,
                fileName);

            return fileMetadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to upload file: {Category}/{Domain}/{EntityType}/{EntityId} - {FileName}",
                options.Category,
                options.Domain,
                options.EntityType,
                options.EntityId,
                fileName);
            throw;
        }
    }

    private static async Task<string> CalculateMD5Async(Stream stream)
    {
        using var md5 = MD5.Create();
        var hash = await md5.ComputeHashAsync(stream);
        return Convert.ToBase64String(hash);
    }

    private static string GenerateShortHash()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 6);
    }

    private FileCategory DetermineCategoryFromContainer(string containerName)
    {
        return _containerMapping.FirstOrDefault(x => x.Value == containerName).Key;
    }

    private (string ContainerName, string BlobName) ExtractContainerAndBlobName(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var pathSegments = uri.AbsolutePath.Trim('/').Split('/');

            if (pathSegments.Length < 2)
            {
                return (string.Empty, string.Empty);
            }

            var containerName = pathSegments[0];
            var blobName = string.Join("/", pathSegments.Skip(1));

            return (containerName, blobName);
        }
        catch
        {
            return (string.Empty, string.Empty);
        }
    }

    private string GenerateBlobName(string fileName, FileUploadOptions options)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        var fileExtension = Path.GetExtension(fileName);
        var hash = GenerateShortHash();
        var environment = _storageSettings.EnvironmentPrefix;

        // Generate path based on category
        switch (options.Category)
        {
            case FileCategory.Avatar:
                // Pattern: {env}/usermanagement/profiles/{userId}/avatar/{yyyy}/{MM}/avatar_{timestamp}_{hash}.{ext}
                return $"{environment}/usermanagement/profiles/{options.EntityId}/avatar/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/avatar_{timestamp}_{hash}{fileExtension}";

            case FileCategory.Document:
            case FileCategory.Report:
                // For project-related files
                if (!string.IsNullOrEmpty(options.ProjectId) && !string.IsNullOrEmpty(options.IncubatorId))
                {
                    var categoryName = options.Category.ToString().ToLowerInvariant();
                    return $"{environment}/{options.IncubatorId}/projects/{options.ProjectId}/{categoryName}s/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{fileExtension}";
                }

                // For system-level files
                return $"{environment}/system/{options.Category.ToString().ToLowerInvariant()}s/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{fileExtension}";

            case FileCategory.Diagnostic:
            case FileCategory.Temporary:
            case FileCategory.PublicAsset:
            default:
                // Generic pattern
                return $"{environment}/system/{options.Category.ToString().ToLowerInvariant()}/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{fileExtension}";
        }
    }

    private string GetCacheControl(FileCategory category)
    {
        return category switch
        {
            FileCategory.Avatar => "public, max-age=31536000", // 1 year
            FileCategory.PublicAsset => "public, max-age=31536000", // 1 year
            FileCategory.Report => "private, max-age=3600", // 1 hour
            FileCategory.Document => "private, no-cache",
            FileCategory.Temporary => "no-store",
            _ => "private"
        };
    }

    private string GetContainerName(FileCategory category)
    {
        return _containerMapping.TryGetValue(category, out var containerName) ? containerName : "temporary";
    }

    private Dictionary<string, string> PrepareBlobTags(FileUploadOptions options)
    {
        var tags = new Dictionary<string, string>
        {
            ["domain"] = options.Domain,
            ["entityType"] = options.EntityType,
            ["entityId"] = options.EntityId,
            ["category"] = options.Category.ToString(),
            ["pii"] = options.ContainsPII.ToString().ToLowerInvariant(),
            ["environment"] = _storageSettings.EnvironmentPrefix
        };

        if (!string.IsNullOrEmpty(options.ProjectId))
        {
            tags["projectId"] = options.ProjectId;
        }

        if (!string.IsNullOrEmpty(options.IncubatorId))
        {
            tags["incubatorId"] = options.IncubatorId;
        }

        // Add custom tags if provided
        if (options.Tags != null)
        {
            // Azure allows max 10 tags
            foreach (var tag in options.Tags.Take(10 - tags.Count))
            {
                if (!tags.ContainsKey(tag.Key))
                {
                    tags[tag.Key] = tag.Value;
                }
            }
        }

        return tags;
    }

    private Dictionary<string, string> PrepareMetadata(FileUploadOptions options, string fileName)
    {
        var metadata = new Dictionary<string, string>
        {
            ["originalFileName"] = fileName,
            ["uploadedAt"] = DateTime.UtcNow.ToString("O"),
            ["uploadedBy"] = options.CustomMetadata != null && options.CustomMetadata.TryGetValue("uploadedBy", out var uploadedBy) ? uploadedBy : "system"
        };

        // Add custom metadata if provided
        if (options.CustomMetadata != null)
        {
            foreach (var item in options.CustomMetadata)
            {
                if (!metadata.ContainsKey(item.Key))
                {
                    metadata[item.Key] = item.Value;
                }
            }
        }

        return metadata;
    }

    private void ValidateFileSize(Stream fileStream, FileCategory category)
    {
        var maxSizeMB = category switch
        {
            FileCategory.Avatar => _storageSettings.MaxAvatarSizeMB,
            FileCategory.Document => _storageSettings.MaxDocumentSizeMB,
            FileCategory.Report => _storageSettings.MaxReportSizeMB,
            _ => 100 // Default max size
        };

        var maxSizeBytes = maxSizeMB * 1024 * 1024;
        if (fileStream.Length > maxSizeBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxSizeMB}MB for {category}");
        }
    }

    private void ValidateUploadOptions(FileUploadOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Domain))
        {
            throw new ArgumentException("Domain is required", nameof(options.Domain));
        }

        if (string.IsNullOrWhiteSpace(options.EntityType))
        {
            throw new ArgumentException("EntityType is required", nameof(options.EntityType));
        }

        if (string.IsNullOrWhiteSpace(options.EntityId))
        {
            throw new ArgumentException("EntityId is required", nameof(options.EntityId));
        }
    }
}
