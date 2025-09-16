using System.Text.Json;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraftOnBehalf;

/// <summary>
/// Command to save a draft of a form submission on behalf of a participant.
/// </summary>
public sealed record SaveDraftOnBehalfCommand : IBaseRequest<long>
{
    /// <summary>
    /// Gets the project ID.
    /// </summary>
    public long ProjectId { get; init; }

    /// <summary>
    /// Gets the participant user ID for whom the form is being filled.
    /// </summary>
    public string ParticipantUserId { get; init; } = null!;

    /// <summary>
    /// Gets the submitter user ID who is filling the form on behalf.
    /// </summary>
    public string SubmittedByUserId { get; init; } = null!;

    /// <summary>
    /// Gets the draft data to save.
    /// </summary>
    public DraftDataDto DraftData { get; init; } = null!;
}

/// <summary>
/// Handler for saving form submission drafts on behalf of participants.
/// </summary>
public sealed class SaveDraftOnBehalfCommandHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<SaveDraftOnBehalfCommandHandler> logger) : BaseCommandHandler<SaveDraftOnBehalfCommand, long>
{
    public override async Task<Result<long>> Handle(SaveDraftOnBehalfCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get project with form submissions
            var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectId), "El proyecto no existe."));
            }

            // Check if the submitter has coordinator/admin access
            var submitterHasAccess = await repository.IsUserProjectCoordinatorAsync(project.Id, request.SubmittedByUserId, cancellationToken);
            if (!submitterHasAccess)
            {
                return Failure(ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource,
                    (nameof(request.SubmittedByUserId), "No tienes permisos de coordinador para completar formularios en nombre de otros."));
            }

            // Check if participant exists and has access to the project
            var participantHasAccess = await repository.IsUserProjectParticipantAsync(project.Id, request.ParticipantUserId, cancellationToken);
            if (!participantHasAccess)
            {
                return Failure(ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource,
                    (nameof(request.ParticipantUserId), "El participante no tiene acceso a este proyecto."));
            }

            // Get or create submission for the participant (on-behalf mode)
            var submission = project.GetOrCreateFormSubmissionOnBehalf(
                request.ParticipantUserId,
                request.SubmittedByUserId,
                timeProvider.UtcNow);

            // Serialize draft data
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var draftJson = JsonSerializer.Serialize(request.DraftData, jsonOptions);

            // Validate JSON size
            const int maxDraftSizeInMb = 10;
            var draftSizeInBytes = System.Text.Encoding.UTF8.GetByteCount(draftJson);
            if (draftSizeInBytes > maxDraftSizeInMb * 1024 * 1024)
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    ("DraftSize", $"El borrador es demasiado grande. Máximo permitido: {maxDraftSizeInMb}MB"));
            }

            // Load project blocks if needed
            if (!project.ProjectBlocks.Any())
            {
                project = await repository.GetProjectWithBlocksByIdAsync(project.Id, cancellationToken);
                if (project is null)
                {
                    return Failure(ResultErrorCodes.Unknown, ("LoadProjectBlocks", "Error al cargar los bloques del proyecto."));
                }
            }

            // Count questions for progress tracking
            int totalRequiredQuestions = project.ProjectBlocks
                .SelectMany(b => b.ProjectQuestions ?? Enumerable.Empty<Domain.Aggregates.BusinessIncubator.ProjectQuestion>())
                .Count(q => q.IsRequired);

            int answeredRequiredQuestions = 0;
            var requiredQuestionIds = project.ProjectBlocks
                .SelectMany(b => b.ProjectQuestions ?? Enumerable.Empty<Domain.Aggregates.BusinessIncubator.ProjectQuestion>())
                .Where(q => q.IsRequired)
                .Select(q => q.Id)
                .ToHashSet();

            foreach (var blockResponse in request.DraftData.BlockResponses)
            {
                foreach (var questionResponse in blockResponse.QuestionResponses)
                {
                    if (questionResponse.IsAnswered && requiredQuestionIds.Contains(questionResponse.QuestionId))
                    {
                        answeredRequiredQuestions++;
                    }
                }
            }

            // Save draft with progress
            submission.SaveDraft(draftJson, answeredRequiredQuestions, totalRequiredQuestions, timeProvider.UtcNow);

            // Persist changes - the submission is already added to the project's collection
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Draft saved on behalf for submission {SubmissionId} by {SubmittedByUserId} for participant {ParticipantUserId} " +
                "with {AnsweredQuestions}/{TotalQuestions} questions answered",
                submission.Id,
                request.SubmittedByUserId,
                request.ParticipantUserId,
                answeredRequiredQuestions,
                totalRequiredQuestions);

            return Success(submission.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving draft on behalf for project {ProjectId}", request.ProjectId);
            return Failure(ResultErrorCodes.Unknown, (string.Empty, "Error al guardar el borrador."));
        }
    }
}