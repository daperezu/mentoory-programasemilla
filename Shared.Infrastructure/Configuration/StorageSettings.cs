namespace LinaSys.Shared.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for blob storage.
/// </summary>
public class StorageSettings
{
    /// <summary>
    /// Gets or sets the environment prefix for blob naming (e.g., "dev", "staging", "prod").
    /// </summary>
    public string EnvironmentPrefix { get; set; } = "dev";

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to enable blob versioning for documents.
    /// </summary>
    public bool EnableDocumentVersioning { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to enable soft delete for documents.
    /// </summary>
    public bool EnableDocumentSoftDelete { get; set; } = true;

    /// <summary>
    /// Gets or sets the soft delete retention period in days.
    /// </summary>
    public int SoftDeleteRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the report retention period in days before archival.
    /// </summary>
    public int ReportRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the temporary file retention period in days.
    /// </summary>
    public int TemporaryFileRetentionDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to calculate and verify MD5 checksums.
    /// </summary>
    public bool EnableMD5Validation { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum file size in MB for avatars.
    /// </summary>
    public int MaxAvatarSizeMB { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum file size in MB for documents.
    /// </summary>
    public int MaxDocumentSizeMB { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum file size in MB for reports.
    /// </summary>
    public int MaxReportSizeMB { get; set; } = 100;
}