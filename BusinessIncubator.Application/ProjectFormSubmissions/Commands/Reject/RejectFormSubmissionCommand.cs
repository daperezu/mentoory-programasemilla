using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Reject;

/// <summary>
/// Command to reject a form submission.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public sealed record RejectFormSubmissionCommand : IBaseRequest
{
    /// <summary>
    /// Gets the project ID.
    /// </summary>
    public long ProjectId { get; init; }

    /// <summary>
    /// Gets the submission ID.
    /// </summary>
    public long SubmissionId { get; init; }

    /// <summary>
    /// Gets the reviewer user ID.
    /// </summary>
    public string ReviewerUserId { get; init; }

    /// <summary>
    /// Gets the rejection reason.
    /// </summary>
    public string RejectionReason { get; init; } = string.Empty;
}
