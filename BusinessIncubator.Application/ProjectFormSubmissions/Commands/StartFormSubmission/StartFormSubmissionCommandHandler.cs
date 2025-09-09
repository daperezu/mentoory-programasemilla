using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.StartFormSubmission;

/// <summary>
/// Handler for starting a new form submission.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="StartFormSubmissionCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public sealed class StartFormSubmissionCommandHandler(IBusinessIncubatorRepository repository, ITimeProvider timeProvider) : BaseCommandHandler<StartFormSubmissionCommand, long>
{

    /// <inheritdoc/>
    public override async Task<Result<long>> Handle(StartFormSubmissionCommand request, CancellationToken cancellationToken)
    {
        // Get the project with form submissions and invitations
        var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectId), "Proyecto no encontrado."));
        }

        try
        {
            // Use the domain method to start form submission
            // This method handles:
            // - Participant access validation
            // - Existing submission checks
            // - Rejected submission re-editing
            // - New submission creation
            var submission = project.StartFormSubmission(
                request.ParticipantUserId,
                timeProvider.UtcNow);

            // Save changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Success(submission.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Failure(
                ResultErrorCodes.Project_ProcessingFailed,
                ("FormSubmission", ex.Message));
        }
    }
}
