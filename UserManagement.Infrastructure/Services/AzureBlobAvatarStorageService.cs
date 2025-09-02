using LinaSys.Shared.Application.Services;
using LinaSys.UserManagement.Application.Services;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage implementation of avatar storage service.
/// Uses the new structured naming pattern for user profiles.
/// </summary>
public class AzureBlobAvatarStorageService(
    IFileStorageService fileStorageService,
    ILogger<AzureBlobAvatarStorageService> logger) : IAvatarStorageService
{
    private readonly IFileStorageService _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
    private readonly ILogger<AzureBlobAvatarStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<string> UploadAvatarAsync(
        string userId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, clean up old avatars for this user
            await CleanupOldAvatarsAsync(userId, cancellationToken);

            // Use the new file storage service with proper metadata
            var uploadOptions = new FileUploadOptions
            {
                Category = FileCategory.Avatar,
                Domain = "UserManagement",
                EntityType = "UserProfile",
                EntityId = userId,
                ContainsPII = true,
                OverwriteIfExists = false,
                Tags = new Dictionary<string, string>
                {
                    ["userId"] = userId
                },
                CustomMetadata = new Dictionary<string, string>
                {
                    ["uploadedBy"] = userId,
                    ["originalFileName"] = fileName
                }
            };

            var fileMetadata = await _fileStorageService.UploadFileAsync(
                fileStream,
                fileName,
                contentType,
                uploadOptions,
                cancellationToken);

            _logger.LogInformation("Avatar uploaded successfully for user {UserId}: {AvatarUrl}", userId, fileMetadata.Url);

            return fileMetadata.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload avatar for user {UserId}", userId);
            throw;
        }
    }

    public async Task DeleteAvatarAsync(string avatarUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                return;
            }

            // Use the new file storage service for deletion
            var deleted = await _fileStorageService.DeleteFileAsync(avatarUrl, cancellationToken);

            if (deleted)
            {
                _logger.LogInformation("Avatar deleted successfully: {AvatarUrl}", avatarUrl);
            }
            else
            {
                _logger.LogWarning("Avatar not found or could not be deleted: {AvatarUrl}", avatarUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete avatar: {AvatarUrl}", avatarUrl);
            // Don't throw - deletion failures shouldn't break the flow
        }
    }

    public async Task<string> GetTemporaryAccessUrlAsync(string avatarUrl, int expirationMinutes = 60)
    {
        try
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                return string.Empty;
            }

            // Use the new file storage service for generating temporary URLs
            var temporaryUrl = await _fileStorageService.GetTemporaryAccessUrlAsync(
                avatarUrl,
                expirationMinutes);

            return temporaryUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate temporary access URL for: {AvatarUrl}", avatarUrl);
            return avatarUrl; // Return original URL as fallback
        }
    }

    private async Task CleanupOldAvatarsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get all existing avatar files for this user
            var existingFiles = await _fileStorageService.ListFilesByEntityAsync(
                "UserManagement",
                "UserProfile",
                userId,
                FileCategory.Avatar,
                cancellationToken);

            // Delete all existing avatars for this user
            foreach (var file in existingFiles)
            {
                await _fileStorageService.DeleteFileAsync(file.Url, cancellationToken);
                _logger.LogInformation("Deleted old avatar for user {UserId}: {Url}", userId, file.Url);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup old avatars for user {UserId}", userId);
            // Don't throw - cleanup failures shouldn't break the upload
        }
    }
}
