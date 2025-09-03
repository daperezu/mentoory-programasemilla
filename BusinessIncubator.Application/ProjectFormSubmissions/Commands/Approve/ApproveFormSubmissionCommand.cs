using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Approve;

/// <summary>
/// Command to approve a form submission.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public sealed record ApproveFormSubmissionCommand : IBaseRequest
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
    /// Gets the approver user ID.
    /// </summary>
    public string ApproverUserId { get; init; }
}
