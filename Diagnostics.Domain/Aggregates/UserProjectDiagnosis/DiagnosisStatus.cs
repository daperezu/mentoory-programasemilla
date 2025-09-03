namespace LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;

/// <summary>
/// Represents the status of a user's project diagnosis.
/// </summary>
public enum DiagnosisStatus
{
    /// <summary>
    /// The diagnosis is in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// The diagnosis has been completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The diagnosis has been archived.
    /// </summary>
    Archived = 3
}