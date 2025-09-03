namespace LinaSys.UserManagement.Application.Services;

/// <summary>
/// Service for managing user avatar storage.
/// </summary>
public interface IAvatarStorageService
{
    /// <summary>
    /// Uploads an avatar image to storage.
    /// </summary>
    /// <param name="userId">The user ID for organizing the avatar.</param>
    /// <param name="fileStream">The file stream containing the image data.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The URL of the uploaded avatar.</returns>
    Task<string> UploadAvatarAsync(
        string userId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an avatar from storage.
    /// </summary>
    /// <param name="avatarUrl">The URL of the avatar to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAvatarAsync(string avatarUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a temporary access URL for an avatar with expiration.
    /// </summary>
    /// <param name="avatarUrl">The avatar URL.</param>
    /// <param name="expirationMinutes">The number of minutes until the URL expires.</param>
    /// <returns>A temporary access URL with SAS token.</returns>
    Task<string> GetTemporaryAccessUrlAsync(string avatarUrl, int expirationMinutes = 60);
}