using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.ApproveWithReview;

/// <summary>
/// Command to approve a form submission with review record creation.
/// Combines both review creation (audit trail) and actual submission approval.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record ApproveFormSubmissionWithReviewCommand(
    long ProjectId,
    long SubmissionId,
    string ApproverUserId,
    string? Comments) : IBaseRequest;