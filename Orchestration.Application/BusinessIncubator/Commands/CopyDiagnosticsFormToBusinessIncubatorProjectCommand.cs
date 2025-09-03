using FluentValidation;
using LinaSys.BusinessIncubator.Application.Project.Commands;
using LinaSys.BusinessIncubator.Application.Project.Queries;
using LinaSys.Diagnostics.Application.Form.Queries;
using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using IBaseRequest = LinaSys.Shared.Application.MediatR.IBaseRequest;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Commands;

public record CopyDiagnosticsFormToBusinessIncubatorProjectCommand(Guid BusinessIncubatorExternalId, long FormId, Guid ProjectExternalId) : IBaseRequest;

public class CopyDiagnosticsFormToBusinessIncubatorProjectCommandValidator : AbstractValidator<CopyDiagnosticsFormToBusinessIncubatorProjectCommand>
{
    public CopyDiagnosticsFormToBusinessIncubatorProjectCommandValidator()
    {
        RuleFor(command => command.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator External ID must not be empty.");
        RuleFor(command => command.FormId)
            .GreaterThan(0)
            .WithMessage("Form ID must be greater than 0.");
        RuleFor(command => command.ProjectExternalId)
            .NotEmpty()
            .WithMessage("Project External ID must not be empty.");
    }
}

public class CopyDiagnosticsFormToBusinessIncubatorProjectCommandHandler(IMediator mediator) : BaseCommandHandler<CopyDiagnosticsFormToBusinessIncubatorProjectCommand>
{
    public override async Task<Result> Handle(CopyDiagnosticsFormToBusinessIncubatorProjectCommand request, CancellationToken cancellationToken)
    {
        var formQueryResult = await mediator.Send(new GetFormWithQuestionsAndAnswersQuery(request.FormId), cancellationToken).ConfigureAwait(false);

        if (formQueryResult.IsFailure)
        {
            return Failure(formQueryResult.ErrorCode ?? ResultErrorCodes.Unknown, formQueryResult.ErrorMessages ?? []);
        }

        if (formQueryResult.Value is null)
        {
            return Failure(ResultErrorCodes.DiagnosisForm_NotFound, (nameof(request.FormId), "The query didn't thrown an error, but the form is empty."));
        }

        var projectExistsResult = await mediator.Send(new ProjectExistsQuery(request.BusinessIncubatorExternalId, request.ProjectExternalId), cancellationToken).ConfigureAwait(false);

        if (projectExistsResult.IsFailure)
        {
            return Failure(projectExistsResult.ErrorCode ?? ResultErrorCodes.Unknown, projectExistsResult.ErrorMessages ?? []);
        }

        if (!projectExistsResult.Value)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project not found."));
        }

        var formToCopy = formQueryResult.Value;

        if (formToCopy.SourceKnowledgeStructureId is not null)
        {
            var knowledgeStructureResult = await mediator.Send(new GetModulesTopicsAndSubjectsFromKnowledgeStructure(formToCopy.SourceKnowledgeStructureId.Value), cancellationToken).ConfigureAwait(false);
            if (knowledgeStructureResult.IsFailure || knowledgeStructureResult.Value is null)
            {
                return Failure(knowledgeStructureResult.ErrorCode ?? ResultErrorCodes.Unknown, knowledgeStructureResult.ErrorMessages ?? []);
            }

            var knowledgeStructureToCopy = knowledgeStructureResult.Value;

            var copyKnowledgeStructureCommand = new CopyKnowledgeStructureCommand(
                request.BusinessIncubatorExternalId,
                request.ProjectExternalId,
                knowledgeStructureToCopy.Id,
                knowledgeStructureToCopy.Name,
                knowledgeStructureToCopy.Description,
                knowledgeStructureToCopy.Modules.Select(module => new KnowledgeStructureModuleToCopyDto(
                    module.Id,
                    module.Name,
                    module.Order,
                    module.Topics.Select(topic => new KnowledgeStructureTopicToCopyDto(
                        topic.Id,
                        topic.Name,
                        topic.Order,
                        topic.Subjects.Select(subject => new KnowledgeStructureSubjectToCopyDto(
                            subject.Id,
                            subject.Title,
                            subject.Content,
                            subject.Order,
                            subject.Resources.Select(resource => new KnowledgeStructureSubjectResourceToCopyDto(
                                resource.Id,
                                resource.Title,
                                resource.Url,
                                resource.Type,
                                resource.EstimatedMinutes,
                                resource.Order)).ToList())).ToList())).ToList())).ToList());

            var copyKnowledgeStructureResult = await mediator.Send(copyKnowledgeStructureCommand, cancellationToken).ConfigureAwait(false);
            if (copyKnowledgeStructureResult.IsFailure)
            {
                return Failure(copyKnowledgeStructureResult.ErrorCode ?? ResultErrorCodes.Unknown, copyKnowledgeStructureResult.ErrorMessages ?? []);
            }
        }
        else
        {
            // Create a minimal knowledge structure for the project
            // The knowledge structure can be empty - modules and topics are optional
            var copyKnowledgeStructureCommand = new CopyKnowledgeStructureCommand(
                request.BusinessIncubatorExternalId,
                request.ProjectExternalId,
                SourceKnowledgeStructureId: null,
                Name: formToCopy.Name,
                Description: $"Estructura creada desde el formulario {formToCopy.Name}",
                Modules: []); // Empty modules list

            var copyKnowledgeStructureResult = await mediator.Send(copyKnowledgeStructureCommand, cancellationToken).ConfigureAwait(false);
            if (copyKnowledgeStructureResult.IsFailure)
            {
                return Failure(copyKnowledgeStructureResult.ErrorCode ?? ResultErrorCodes.Unknown, copyKnowledgeStructureResult.ErrorMessages ?? []);
            }
        }

        var copyBlocksCommand = new CopyBlocksCommand(
                request.BusinessIncubatorExternalId,
                request.ProjectExternalId,
                formToCopy.Questions
                    .Select(q => new ProjectBlockToCopyDto(q.BlockId, q.BlockName))
                    .DistinctBy(block => new { block.SourceBlockId, block.Name })

                    .ToList());

        var copyBlocksResult = await mediator.Send(copyBlocksCommand, cancellationToken).ConfigureAwait(false);
        if (copyBlocksResult.IsFailure)
        {
            return Failure(copyBlocksResult.ErrorCode ?? ResultErrorCodes.Unknown, copyBlocksResult.ErrorMessages ?? []);
        }

        var copyQuestionsCommand = new CopyQuestionsCommand(
            request.BusinessIncubatorExternalId,
            request.ProjectExternalId,
            formToCopy.Questions
                .Select(q => new QuestionToCopyDto(
                    q.TopicId,
                    q.BlockId,
                    q.Id,
                    q.Text,
                    q.AnswerType,
                    q.AppliesToPhase,
                    q.IsUsedForMentoringPlan,
                    q.IsUsedForDiagnosis,
                    q.Order,
                    q.Answers.Select(a => new AnswerOptionToCopyDto(
                        a.Id,
                        a.Text,
                        a.Score,
                        a.Foda.ToString(),
                        a.FodaExplanation,
                        a.Odsr.ToString(),
                        a.OdsrExplanation,
                        a.Order,
                        a.FollowUpQuestionText)).ToList()))
                .DistinctBy(question => new { question.SourceQuestionId, question.Text, question.SourceBlockId, question.SourceTopicId })
                .ToList());

        var copyQuestionsResult = await mediator.Send(copyQuestionsCommand, cancellationToken).ConfigureAwait(false);
        if (copyQuestionsResult.IsFailure)
        {
            return Failure(copyQuestionsResult.ErrorCode ?? ResultErrorCodes.Unknown, copyQuestionsResult.ErrorMessages ?? []);
        }

        return Success();
    }
}
