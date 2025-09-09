using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get available forms for a user in a project.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="ProjectId">The project identifier.</param>
public record GetAvailableFormsQuery(string UserId, long ProjectId)
    : IBaseRequest<List<AvailableFormDto>>;

/// <summary>
/// DTO for available form information.
/// </summary>
public class AvailableFormDto
{
    /// <summary>
    /// Gets or sets the existing form identifier if created.
    /// </summary>
    public Guid? ExistingFormId { get; set; }

    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the question phase.
    /// </summary>
    public QuestionPhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the stage name.
    /// </summary>
    public string StageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stage type.
    /// </summary>
    public ProjectStageType StageType { get; set; }

    /// <summary>
    /// Gets or sets the due date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the form is created.
    /// </summary>
    public bool IsCreated { get; set; }

    /// <summary>
    /// Gets or sets the form submission status.
    /// </summary>
    public ProjectFormSubmissionStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public double CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the form can be started.
    /// </summary>
    public bool CanStart { get; set; }

    /// <summary>
    /// Gets or sets the form ID (template ID).
    /// </summary>
    public long? FormId { get; set; }

    /// <summary>
    /// Gets or sets the number of pending feedback items requiring response.
    /// </summary>
    public int PendingFeedbackCount { get; set; }
}

/// <summary>
/// Handler for GetAvailableFormsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetAvailableFormsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
/// <param name="timeProvider">The time provider.</param>
public class GetAvailableFormsQueryHandler(
    IBusinessIncubatorRepository repository,
    TimeProvider timeProvider)
    : BaseCommandHandler<GetAvailableFormsQuery, List<AvailableFormDto>>
{
    /// <inheritdoc/>
    public override async Task<Result<List<AvailableFormDto>>> Handle(
        GetAvailableFormsQuery request,
        CancellationToken cancellationToken)
    {
        var availableForms = new List<AvailableFormDto>();

        // Get project with stages
        var project = await repository.GetProjectWithStagesAsync(request.ProjectId, cancellationToken);
        if (project is null || project.IsDeleted)
        {
            return Success(availableForms);
        }

        // Check if user is participant
        var isParticipant = await repository.IsUserProjectParticipantAsync(
            request.ProjectId, request.UserId, cancellationToken);
        if (!isParticipant)
        {
            return Success(availableForms);
        }

        // Get project knowledge structure
        var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(
            request.ProjectId, cancellationToken);
        if (knowledgeStructure is null)
        {
            return Success(availableForms); // No forms without structure
        }

        // Get existing form submissions for user
        var existingForms = await repository.GetProjectFormSubmissionsByUserAsync(
            request.ProjectId, request.UserId, cancellationToken);

        var currentDate = timeProvider.GetUtcNow().DateTime;

        // Check each stage that can have forms
        foreach (var stage in project.ProjectStages.Where(s => s.IsActive))
        {
            // Only InitialFormCollection and FinalFormCollection have forms
            if (stage.Type != ProjectStageType.InitialFormCollection &&
                stage.Type != ProjectStageType.FinalFormCollection)
            {
                continue;
            }

            var phase = ProjectFormSubmission.GetPhaseForStage(stage.Type);
            if (phase == QuestionPhase.Undefined)
            {
                continue;
            }

            // Check if form exists
            var existingForm = existingForms.FirstOrDefault(f => f.Phase == phase);

            // Get pending feedback count if form exists
            var pendingFeedbackCount = 0;
            if (existingForm is not null)
            {
                var feedbackList = await repository.GetFeedbackWithRepliesForSubmissionAsync(
                    existingForm.Id, cancellationToken);
                pendingFeedbackCount = feedbackList.Count(f =>
                    f.Status == FeedbackStatus.ReviewNeeded && !f.IsDeleted);
            }

            var dto = new AvailableFormDto
            {
                ExistingFormId = existingForm?.ExternalId,
                ProjectId = project.Id,
                ProjectName = project.Name,
                Phase = phase,
                StageName = stage.Title,
                StageType = stage.Type,
                DueDate = stage.EndDate,
                IsCreated = existingForm is not null,
                Status = existingForm?.Status,
                CompletionPercentage = existingForm?.CompletionPercentage ?? 0,
                CanStart = stage.IsWithinPeriod(currentDate),
                FormId = knowledgeStructure.Id, // Template ID
                PendingFeedbackCount = pendingFeedbackCount
            };

            availableForms.Add(dto);
        }

        return Success(availableForms);
    }
}