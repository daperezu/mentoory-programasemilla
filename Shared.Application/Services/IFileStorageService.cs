namespace LinaSys.Shared.Application.Services;

/// <summary>
/// Service for managing file storage across different categories and domains.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage with structured naming and metadata.
    /// </summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="options">Upload options including category, domain, entity info.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Metadata about the uploaded file.</returns>
    Task<FileMetadata> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        FileUploadOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="fileUrl">The URL of the file to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if deleted successfully, false otherwise.</returns>
    Task<bool> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a temporary access URL for a file with expiration.
    /// </summary>
    /// <param name="fileUrl">The file URL.</param>
    /// <param name="expirationMinutes">The number of minutes until the URL expires.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A temporary access URL with SAS token if needed.</returns>
    Task<string> GetTemporaryAccessUrlAsync(
        string fileUrl,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata for a file.
    /// </summary>
    /// <param name="fileUrl">The file URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The file metadata, or null if not found.</returns>
    Task<FileMetadata?> GetFileMetadataAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from storage.
    /// </summary>
    /// <param name="fileUrl">The file URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream containing the file contents.</returns>
    Task<Stream?> DownloadFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists files by entity.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="category">Optional file category filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of file metadata.</returns>
    Task<IReadOnlyList<FileMetadata>> ListFilesByEntityAsync(
        string domain,
        string entityType,
        string entityId,
        FileCategory? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="fileUrl">The file URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    Task<bool> FileExistsAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);
}
