using System.Text.Json;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;

/// <summary>
/// Command to save a draft of a form submission.
/// </summary>
public sealed record SaveDraftCommand : IBaseRequest<long>
{
    /// <summary>
    /// Gets the project ID.
    /// </summary>
    public long ProjectId { get; init; }

    /// <summary>
    /// Gets the participant user ID.
    /// </summary>
    public string ParticipantUserId { get; init; }

    /// <summary>
    /// Gets the draft data to save.
    /// </summary>
    public DraftDataDto DraftData { get; init; } = null!;
}

/// <summary>
/// Handler for saving form submission drafts.
/// </summary>
public sealed class SaveDraftCommandHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<SaveDraftCommandHandler> logger) : BaseCommandHandler<SaveDraftCommand, long>
{

    public override async Task<Result<long>> Handle(SaveDraftCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate draft data

            // Get project with form submissions
            var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectId), "El proyecto no existe."));
            }

            // Check if participant has access through UserProjectAccess
            var hasAccess = await repository.IsUserProjectParticipantAsync(project.Id, request.ParticipantUserId, cancellationToken);
            if (!hasAccess)
            {
                return Failure(ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource, (nameof(request.ParticipantUserId), "No tienes acceso a este proyecto."));
            }

            // Start or get existing submission
            var submission = project.StartFormSubmission(
                request.ParticipantUserId,
                timeProvider.UtcNow);

            // Serialize draft data with options for better formatting
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false, // Compact JSON to save space
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var draftJson = JsonSerializer.Serialize(request.DraftData, jsonOptions);

            // Validate JSON size (SQL Server nvarchar(max) can hold ~1GB, but let's be reasonable)
            const int maxDraftSizeInMb = 10;
            var draftSizeInBytes = System.Text.Encoding.UTF8.GetByteCount(draftJson);
            if (draftSizeInBytes > maxDraftSizeInMb * 1024 * 1024)
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("DraftSize", $"El borrador es demasiado grande. Máximo permitido: {maxDraftSizeInMb}MB"));
            }

            // Always load project blocks to get accurate total question count
            if (!project.ProjectBlocks.Any())
            {
                project = await repository.GetProjectWithBlocksByIdAsync(project.Id, cancellationToken);
                if (project is null)
                {
                    return Failure(ResultErrorCodes.Unknown, ("LoadProjectBlocks", "Error al cargar los bloques del proyecto."));
                }
            }

            // Count ONLY REQUIRED questions to match frontend logic
            // Frontend only counts required questions for progress calculation
            int totalRequiredQuestions = project.ProjectBlocks
                .SelectMany(b => b.ProjectQuestions ?? Enumerable.Empty<Domain.Aggregates.BusinessIncubator.ProjectQuestion>())
                .Count(q => q.IsRequired);

            // Count answered questions from the draft data
            // Only count answers for required questions to match frontend
            int answeredRequiredQuestions = 0;
            foreach (var block in request.DraftData.BlockResponses)
            {
                foreach (var questionResponse in block.QuestionResponses.Where(q => q.IsAnswered))
                {
                    // Find if this question is required in the structure
                    var projectBlock = project.ProjectBlocks.FirstOrDefault(b => b.Id == block.BlockId);
                    if (projectBlock is not null)
                    {
                        var question = projectBlock.ProjectQuestions?.FirstOrDefault(q => q.Id == questionResponse.QuestionId);
                        if (question is not null && question.IsRequired)
                        {
                            answeredRequiredQuestions++;
                        }
                    }
                }
            }

            // Use total questions for storage (not just required) but calculate percentage based on required
            int totalQuestions = project.ProjectBlocks.Sum(b => b.ProjectQuestions?.Count ?? 0);

            // If no required questions, fall back to all questions
            if (totalRequiredQuestions == 0)
            {
                totalRequiredQuestions = Math.Max(totalQuestions, 1);
                answeredRequiredQuestions = request.DraftData.BlockResponses
                    .SelectMany(b => b.QuestionResponses)
                    .Count(q => q.IsAnswered);
            }

            // Save draft with progress tracking - use required questions for accurate progress
            submission.SaveDraft(draftJson, answeredRequiredQuestions, totalRequiredQuestions, timeProvider.UtcNow);

            // Save changes - update the project which will cascade to save submissions
            // For new submissions, EF Core will track them as Added
            // For existing submissions, they'll be tracked as Modified
            await repository.UpdateAsync(project, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Draft saved for participant {ParticipantUserId} in project {ProjectId}, size: {SizeKB}KB",
                request.ParticipantUserId,
                request.ProjectId,
                draftSizeInBytes / 1024);

            return Success(submission.Id);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation saving draft");
            return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("BusinessRule", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving draft for project {ProjectId}", request.ProjectId);
            return Failure(ResultErrorCodes.Unknown, ("SaveDraft", "Error al guardar el borrador. Por favor, intenta nuevamente."));
        }
    }
}
