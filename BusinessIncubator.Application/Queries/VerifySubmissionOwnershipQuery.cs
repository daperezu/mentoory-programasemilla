using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to verify if a user owns a specific form submission.
/// </summary>
public record VerifySubmissionOwnershipQuery(long ProjectId, long SubmissionId, string UserId) : IBaseRequest<bool>;

/// <summary>
/// Handler for VerifySubmissionOwnershipQuery.
/// </summary>
public class VerifySubmissionOwnershipQueryHandler(IBusinessIncubatorRepository repository)
    : BaseCommandHandler<VerifySubmissionOwnershipQuery, bool>
{
    public override async Task<Result<bool>> Handle(
        VerifySubmissionOwnershipQuery request,
        CancellationToken cancellationToken)
    {
        var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Success(false);
        }

        var submission = project.GetFormSubmission(request.SubmissionId);
        var ownsSubmission = submission is not null && submission.ParticipantUserId == request.UserId;

        return Success(ownsSubmission);
    }
}