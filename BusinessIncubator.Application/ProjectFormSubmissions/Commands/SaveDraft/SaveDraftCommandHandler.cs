using System.Text.Json;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;

/// <summary>
/// Handler for saving form submission drafts.
/// </summary>
public sealed class SaveDraftCommandHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<SaveDraftCommandHandler> logger) : BaseCommandHandler<SaveDraftCommand>
{

    public override async Task<Result> Handle(SaveDraftCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate draft data
            if (request.DraftData is null)
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, (nameof(request.DraftData), "Los datos del borrador no pueden estar vacíos."));
            }

            // Get project with form submissions
            var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectId), "El proyecto no existe."));
            }

            // Check if participant has access
            if (!project.HasFormAccess(request.ParticipantUserId))
            {
                return Failure(ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource, (nameof(request.ParticipantUserId), "No tienes acceso a este proyecto."));
            }

            // Start or get existing submission
            var submission = project.StartFormSubmission(
                request.ParticipantUserId,
                request.FormId,
                timeProvider.UtcNow);

            // Serialize draft data with options for better formatting
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false, // Compact JSON to save space
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var draftJson = JsonSerializer.Serialize(request.DraftData, jsonOptions);

            // Validate JSON size (SQL Server nvarchar(max) can hold ~1GB, but let's be reasonable)
            const int maxDraftSizeInMB = 10;
            var draftSizeInBytes = System.Text.Encoding.UTF8.GetByteCount(draftJson);
            if (draftSizeInBytes > maxDraftSizeInMB * 1024 * 1024)
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("DraftSize", $"El borrador es demasiado grande. Máximo permitido: {maxDraftSizeInMB}MB"));
            }

            // Save draft with progress tracking
            // For now, we'll pass default values for answered/total questions
            // These should ideally come from the request or be calculated from the draft
            submission.SaveDraft(draftJson, 0, 0, timeProvider.UtcNow);

            // Save changes - update submission, not project
            await repository.UpdateSubmissionAsync(submission, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Draft saved for participant {ParticipantUserId} in project {ProjectId}, size: {SizeKB}KB",
                request.ParticipantUserId,
                request.ProjectId,
                draftSizeInBytes / 1024);

            return Success();
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