using FluentValidation;
using LinaSys.BusinessIncubator.Application.Project.Queries;
using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using IBaseRequest = LinaSys.Shared.Application.MediatR.IBaseRequest;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to copy knowledge structure from one project to another within the same business incubator.
/// </summary>
public record CopyProjectKnowledgeStructureToProjectCommand(
    Guid BusinessIncubatorExternalId,
    long SourceProjectId,
    Guid TargetProjectExternalId) : IBaseRequest;

/// <summary>
/// Validator for CopyProjectKnowledgeStructureToProjectCommand.
/// </summary>
public class CopyProjectKnowledgeStructureToProjectCommandValidator : AbstractValidator<CopyProjectKnowledgeStructureToProjectCommand>
{
    public CopyProjectKnowledgeStructureToProjectCommandValidator()
    {
        RuleFor(command => command.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator External ID must not be empty.");

        RuleFor(command => command.SourceProjectId)
            .GreaterThan(0)
            .WithMessage("Source Project ID must be greater than 0.");

        RuleFor(command => command.TargetProjectExternalId)
            .NotEmpty()
            .WithMessage("Target Project External ID must not be empty.");
    }
}

/// <summary>
/// Handler for CopyProjectKnowledgeStructureToProjectCommand.
/// </summary>
public class CopyProjectKnowledgeStructureToProjectCommandHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext,
    IMediator mediator) : BaseCommandHandler<CopyProjectKnowledgeStructureToProjectCommand>
{
    public override async Task<Result> Handle(
        CopyProjectKnowledgeStructureToProjectCommand request,
        CancellationToken cancellationToken)
    {
        // Verify target project exists and belongs to the business incubator
        var projectExistsResult = await mediator.Send(
            new ProjectExistsQuery(request.BusinessIncubatorExternalId, request.TargetProjectExternalId),
            cancellationToken);

        if (projectExistsResult.IsFailure || !projectExistsResult.Value)
        {
            return Failure(ResultErrorCodes.Project_NotFound,
                (nameof(request.TargetProjectExternalId), "Target project not found."));
        }

        // Get source project with its knowledge structure
        var sourceProject = await repository.GetProjectWithKnowledgeStructureByIdAsync(
            request.SourceProjectId, cancellationToken);

        if (sourceProject is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound,
                (nameof(request.SourceProjectId), "Source project not found."));
        }

        // Get the knowledge structure separately since it's not directly accessible
        var sourceStructure = await repository.GetProjectKnowledgeStructureAsync(
            sourceProject.Id, cancellationToken);

        if (sourceStructure is null)
        {
            return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                ("SourceProject", "Source project does not have a knowledge structure."));
        }

        // Get target project
        var targetProject = await repository.GetProjectByExternalIdAsync(
            request.TargetProjectExternalId, cancellationToken);

        if (targetProject is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound,
                (nameof(request.TargetProjectExternalId), "Target project not found."));
        }

        // Verify both projects belong to the same business incubator
        if (sourceProject.BusinessIncubatorId != targetProject.BusinessIncubatorId)
        {
            return Failure(ResultErrorCodes.Unknown,
                ("BusinessIncubator", "Projects must belong to the same business incubator."));
        }

        // Check if target project already has a knowledge structure
        var existingStructure = await repository.GetProjectKnowledgeStructureAsync(
            targetProject.Id, cancellationToken);

        if (existingStructure is not null)
        {
            return Failure(ResultErrorCodes.Project_AlreadyAssigned,
                ("TargetProject", "Target project already has a knowledge structure."));
        }

        // Set source form reference (using the source project's internal ID as reference)
        targetProject.SetSourceForm(sourceProject.Id, auditContext);

        // Create the project knowledge structure
        var newStructure = targetProject.SetKnowledgeStructure(
            sourceStructure.SourceKnowledgeStructureId,
            sourceStructure.Name,
            false, // not customized initially
            sourceStructure.Description,
            false, // not customized initially
            auditContext);

        // Copy modules with their topics and subjects
        foreach (var sourceModule in sourceStructure.ProjectModules.OrderBy(m => m.Order))
        {
            var newModule = newStructure.AddProjectModule(
                sourceModule.SourceModuleId,
                sourceModule.Name,
                false, // not customized
                sourceModule.Order,
                false); // not customized

            // Copy topics
            foreach (var sourceTopic in sourceModule.ProjectTopics.OrderBy(t => t.Order))
            {
                var newTopic = newModule.AddProjectTopic(
                    sourceTopic.SourceTopicId,
                    sourceTopic.Name,
                    false, // not customized
                    sourceTopic.Order,
                    false); // not customized

                // Copy subjects
                foreach (var sourceSubject in sourceTopic.ProjectSubjects.OrderBy(s => s.Order))
                {
                    var newSubject = newTopic.AddProjectSubject(
                        sourceSubject.SourceSubjectId,
                        sourceSubject.Title,
                        false, // not customized
                        sourceSubject.Content,
                        false, // not customized
                        sourceSubject.Order,
                        false); // not customized

                    // Copy subject resources
                    foreach (var sourceResource in sourceSubject.ProjectSubjectResources.OrderBy(r => r.Order))
                    {
                        newSubject.AddProjectSubjectResource(
                            sourceResource.SourceSubjectResourceId,
                            sourceResource.Title,
                            false, // not customized
                            sourceResource.Url,
                            false, // not customized
                            sourceResource.Type,
                            false, // not customized
                            sourceResource.EstimatedMinutes,
                            false, // not customized
                            sourceResource.Order,
                            false); // not customized
                    }
                }
            }
        }

        // Copy blocks
        var sourceProjectWithBlocks = await repository.GetProjectWithBlocksByIdAsync(sourceProject.Id, cancellationToken);
        if (sourceProjectWithBlocks?.ProjectBlocks is not null)
        {
            foreach (var sourceBlock in sourceProjectWithBlocks.ProjectBlocks.OrderBy(b => b.Name))
            {
                targetProject.AddBlock(
                    sourceBlock.Name,
                    sourceBlock.SourceBlockId,
                    auditContext);
            }
        }

        // Save the changes so far to get all the IDs
        repository.Update(targetProject);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Now copy all questions - reload to get the complete structure with questions
        var sourceWithQuestions = await repository.GetProjectWithBlocksByIdAsync(
            sourceProject.Id, cancellationToken);

        if (sourceWithQuestions?.ProjectBlocks is not null && sourceWithQuestions.ProjectBlocks.Any())
        {
            // Reload target to get the newly created structure
            var targetWithStructure = await repository.GetProjectWithKnowledgeStructureByIdAsync(
                targetProject.Id, cancellationToken);

            var targetKnowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(
                targetProject.Id, cancellationToken);

            if (targetWithStructure is not null && targetKnowledgeStructure is not null)
            {
                foreach (var sourceBlock in sourceWithQuestions.ProjectBlocks)
                {
                    // Find the corresponding target block by source ID
                    var targetBlock = targetWithStructure.ProjectBlocks
                        .FirstOrDefault(b => b.SourceBlockId == sourceBlock.SourceBlockId ||
                                           (sourceBlock.SourceBlockId is null && b.Name == sourceBlock.Name));

                    if (targetBlock is not null)
                    {
                        foreach (var sourceQuestion in sourceBlock.ProjectQuestions.OrderBy(q => q.Order))
                        {
                            // Find the corresponding target topic
                            ProjectTopic? targetTopic = null;
                            if (sourceQuestion.ProjectTopic is not null)
                            {
                                // First try to match by source ID
                                if (sourceQuestion.ProjectTopic.SourceTopicId is not null)
                                {
                                    targetTopic = targetKnowledgeStructure
                                        .FindTopicBySourceId(sourceQuestion.ProjectTopic.SourceTopicId.Value);
                                }

                                // If not found and it's a custom topic, match by name
                                if (targetTopic is null)
                                {
                                    targetTopic = targetKnowledgeStructure.ProjectModules
                                        .SelectMany(m => m.ProjectTopics)
                                        .FirstOrDefault(t => t.Name == sourceQuestion.ProjectTopic.Name);
                                }
                            }

                            if (targetTopic is not null)
                            {
                                // Clone the question with all its properties
                                var newQuestion = targetTopic.AddProjectQuestion(
                                    targetBlock.Id,
                                    sourceQuestion.SourceQuestionId, // Keep original source ID (could be null)
                                    sourceQuestion.Text,
                                    sourceQuestion.IsTextCustomized,
                                    sourceQuestion.AnswerType,
                                    sourceQuestion.IsAnswerTypeCustomized,
                                    sourceQuestion.AppliesToPhase,
                                    sourceQuestion.IsAppliesToPhaseCustomized,
                                    sourceQuestion.IsUsedForMentoringPlan,
                                    sourceQuestion.IsMentoringPlanCustomized,
                                    sourceQuestion.IsUsedForDiagnosis,
                                    sourceQuestion.IsDiagnosisCustomized,
                                    sourceQuestion.Order,
                                    sourceQuestion.IsOrderCustomized);

                                // Clone all answer options
                                foreach (var sourceAnswer in sourceQuestion.ProjectAnswerOptions.OrderBy(a => a.Order))
                                {
                                    newQuestion.AddProjectAnswerOption(
                                        sourceAnswer.SourceAnswerOptionId, // Keep original source ID (could be null)
                                        sourceAnswer.Text,
                                        sourceAnswer.IsTextCustomized,
                                        sourceAnswer.Score,
                                        sourceAnswer.IsScoreCustomized,
                                        sourceAnswer.Foda,
                                        sourceAnswer.IsFodaCustomized,
                                        sourceAnswer.FodaExplanation,
                                        sourceAnswer.IsFodaExplanationCustomized,
                                        sourceAnswer.Odsr,
                                        sourceAnswer.IsOdsrCustomized,
                                        sourceAnswer.OdsrExplanation,
                                        sourceAnswer.IsOdsrExplanationCustomized,
                                        sourceAnswer.Order,
                                        sourceAnswer.IsOrderCustomized,
                                        sourceAnswer.FollowUpQuestionText,
                                        sourceAnswer.IsFollowUpTextCustomized);
                                }
                            }
                        }
                    }
                }

                // Save the questions
                repository.Update(targetWithStructure);
                await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return Success();
    }
}
