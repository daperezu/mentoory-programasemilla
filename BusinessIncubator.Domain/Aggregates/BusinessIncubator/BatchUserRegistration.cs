using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Represents a batch user registration operation for a project.
/// </summary>
public class BatchUserRegistration : SoftDeletableEntity
{
    protected BatchUserRegistration()
    {
    }

    private BatchUserRegistration(long projectId, string fileName, int totalUsers)
    {
        ExternalId = Guid.NewGuid();
        ProjectId = projectId;
        FileName = fileName;
        TotalUsers = totalUsers;
        Status = BatchUserRegistrationStatus.Pending;
        ProcessedUsers = 0;
        SuccessfulUsers = 0;
        FailedUsers = 0;
    }

    /// <summary>
    /// Gets the external identifier for this batch registration.
    /// </summary>
    public Guid ExternalId { get; private set; }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// Gets the name of the uploaded CSV file.
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the total number of users in the batch.
    /// </summary>
    public int TotalUsers { get; private set; }

    /// <summary>
    /// Gets the number of users that have been processed.
    /// </summary>
    public int ProcessedUsers { get; private set; }

    /// <summary>
    /// Gets the number of users that were successfully registered.
    /// </summary>
    public int SuccessfulUsers { get; private set; }

    /// <summary>
    /// Gets the number of users that failed to register.
    /// </summary>
    public int FailedUsers { get; private set; }

    /// <summary>
    /// Gets the status of the batch registration.
    /// </summary>
    public BatchUserRegistrationStatus Status { get; private set; } = BatchUserRegistrationStatus.Pending;

    /// <summary>
    /// Gets the date when processing started.
    /// </summary>
    public DateTime? ProcessingStartedAt { get; private set; }

    /// <summary>
    /// Gets the date when processing completed.
    /// </summary>
    public DateTime? ProcessingCompletedAt { get; private set; }

    /// <summary>
    /// Gets the error details if the batch registration failed.
    /// </summary>
    public string? ErrorDetails { get; private set; }

    /// <summary>
    /// Navigation property for EF Core.
    /// </summary>
    internal virtual Project Project { get; private set; } = null!;

    /// <summary>
    /// Starts the processing of the batch registration.
    /// </summary>
    /// <param name="startedAt">The processing start timestamp.</param>
    public void StartProcessing(DateTime startedAt)
    {
        if (Status != BatchUserRegistrationStatus.Pending)
        {
            throw new InvalidOperationException("Can only start processing from Pending status.");
        }

        Status = BatchUserRegistrationStatus.Processing;
        ProcessingStartedAt = startedAt;
    }

    /// <summary>
    /// Updates the progress of the batch registration.
    /// </summary>
    /// <param name="processedUsers">Number of users processed.</param>
    /// <param name="successfulUsers">Number of users successfully registered.</param>
    /// <param name="failedUsers">Number of users that failed to register.</param>
    public void UpdateProgress(int processedUsers, int successfulUsers, int failedUsers)
    {
        if (Status != BatchUserRegistrationStatus.Processing)
        {
            throw new InvalidOperationException("Can only update progress during Processing status.");
        }

        ProcessedUsers = processedUsers;
        SuccessfulUsers = successfulUsers;
        FailedUsers = failedUsers;

        if (ProcessedUsers > TotalUsers)
        {
            throw new InvalidOperationException("Processed users cannot exceed total users.");
        }
    }

    /// <summary>
    /// Completes the batch registration successfully.
    /// </summary>
    /// <param name="completedAt">The completion timestamp.</param>
    public void Complete(DateTime completedAt)
    {
        if (Status != BatchUserRegistrationStatus.Processing)
        {
            throw new InvalidOperationException("Can only complete from Processing status.");
        }

        ProcessingCompletedAt = completedAt;

        if (FailedUsers == 0)
        {
            Status = BatchUserRegistrationStatus.Completed;
        }
        else if (SuccessfulUsers > 0)
        {
            Status = BatchUserRegistrationStatus.PartiallyCompleted;
        }
        else
        {
            Status = BatchUserRegistrationStatus.Failed;
        }
    }

    /// <summary>
    /// Marks the batch registration as failed.
    /// </summary>
    /// <param name="errorDetails">The error details.</param>
    /// <param name="failedAt">The failure timestamp.</param>
    public void MarkAsFailed(string errorDetails, DateTime failedAt)
    {
        Status = BatchUserRegistrationStatus.Failed;
        ProcessingCompletedAt = failedAt;
        ErrorDetails = errorDetails;
    }

    /// <summary>
    /// Gets the completion percentage.
    /// </summary>
    /// <returns>The completion percentage as a value between 0 and 100.</returns>
    public double GetCompletionPercentage()
    {
        if (TotalUsers == 0)
        {
            return 0;
        }

        return (double)ProcessedUsers / TotalUsers * 100;
    }

    /// <summary>
    /// Gets the success rate percentage.
    /// </summary>
    /// <returns>The success rate percentage as a value between 0 and 100.</returns>
    public double GetSuccessRate()
    {
        if (ProcessedUsers == 0)
        {
            return 0;
        }

        return (double)SuccessfulUsers / ProcessedUsers * 100;
    }

    /// <summary>
    /// Creates a new batch user registration with proper audit information.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="fileName">The name of the uploaded CSV file.</param>
    /// <param name="totalUsers">The total number of users to register.</param>
    /// <param name="auditContext">The audit context.</param>
    /// <returns>A new batch user registration.</returns>
    internal static BatchUserRegistration Create(
        long projectId,
        string fileName,
        int totalUsers,
        IAuditContext auditContext)
    {
        var batchRegistration = new BatchUserRegistration(projectId, fileName.Trim(), totalUsers);
        batchRegistration.SetCreated(auditContext);
        return batchRegistration;
    }
}
