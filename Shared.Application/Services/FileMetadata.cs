using System.Collections.Generic;

namespace LinaSys.Shared.Application.Services;

/// <summary>
/// Represents metadata for a stored file.
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the file.
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the blob URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content type (MIME type).
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    /// Gets or sets the MD5 hash of the content.
    /// </summary>
    public string? ContentMD5 { get; set; }

    /// <summary>
    /// Gets or sets the file category.
    /// </summary>
    public FileCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the domain that owns this file.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type associated with this file.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity ID associated with this file.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this file contains PII.
    /// </summary>
    public bool ContainsPII { get; set; }

    /// <summary>
    /// Gets or sets when the file was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Gets or sets who uploaded the file.
    /// </summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets custom tags for the file.
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets custom metadata properties.
    /// </summary>
    public Dictionary<string, string> CustomMetadata { get; set; } = new();
}

/// <summary>
/// Options for file upload operations.
/// </summary>
public class FileUploadOptions
{
    /// <summary>
    /// Gets or sets the file category.
    /// </summary>
    public FileCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the domain that owns this file.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type associated with this file.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity ID associated with this file.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator ID (for project-related files).
    /// </summary>
    public string? IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the project ID (for project-related files).
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets whether this file contains PII.
    /// </summary>
    public bool ContainsPII { get; set; }

    /// <summary>
    /// Gets or sets whether to overwrite if file exists.
    /// </summary>
    public bool OverwriteIfExists { get; set; }

    /// <summary>
    /// Gets or sets custom tags for the file.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets custom metadata properties.
    /// </summary>
    public Dictionary<string, string>? CustomMetadata { get; set; }
}