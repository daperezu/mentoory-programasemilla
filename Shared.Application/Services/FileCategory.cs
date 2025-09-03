namespace LinaSys.Shared.Application.Services;

/// <summary>
/// Represents the category of a file for storage organization.
/// </summary>
public enum FileCategory
{
    /// <summary>
    /// User profile avatars.
    /// </summary>
    Avatar,

    /// <summary>
    /// Business documents (contracts, agreements, etc.).
    /// </summary>
    Document,

    /// <summary>
    /// Generated reports (Excel, CSV, PDF).
    /// </summary>
    Report,

    /// <summary>
    /// Diagnostic logs and debug dumps.
    /// </summary>
    Diagnostic,

    /// <summary>
    /// Temporary files with short retention.
    /// </summary>
    Temporary,

    /// <summary>
    /// Public assets (non-PII static content).
    /// </summary>
    PublicAsset
}