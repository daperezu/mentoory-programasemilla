using FluentValidation;
using LinaSys.KnowledgeStructure.Application.Subject.Commands;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Orchestration.Application.KnowledgeStructure.Commands;

public sealed record CreateSubjectOrchestrationCommand(
    string Title,
    string? Content,
    long StructureTopicId) : IBaseRequest<long>;

public class CreateSubjectOrchestrationCommandValidator : AbstractValidator<CreateSubjectOrchestrationCommand>
{
    public CreateSubjectOrchestrationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido.")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");

        RuleFor(x => x.StructureTopicId)
            .GreaterThan(0).WithMessage("El tema es requerido.");

        RuleFor(x => x.Content)
            .MaximumLength(2000).WithMessage("El contenido no puede exceder 2000 caracteres.");
    }
}

public sealed class CreateSubjectOrchestrationCommandHandler(
    IMediator mediator,
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ISubjectRepository subjectRepository) : BaseCommandHandler<CreateSubjectOrchestrationCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateSubjectOrchestrationCommand request,
        CancellationToken cancellationToken)
    {
        // Get the structure topic
        var structureTopic = await knowledgeStructureRepository.GetStructureTopicWithSubjectsAsync(
            request.StructureTopicId,
            cancellationToken);

        if (structureTopic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(request.StructureTopicId), "No se encontró el tema especificado."));
        }

        // Create the subject through its own aggregate
        var createSubjectCommand = new CreateSubjectCommand(request.Title, request.Content);
        var subjectResult = await mediator.Send(createSubjectCommand, cancellationToken);

        if (!subjectResult.IsSuccess)
        {
            return Failure(subjectResult.ErrorCode ?? ResultErrorCodes.Unknown, subjectResult.ErrorMessages ?? []);
        }

        var subjectId = subjectResult.Value;
        var subject = await subjectRepository.FindByIdAsync(subjectId, cancellationToken);

        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                ("Subject", "La asignatura creada no fue encontrada."));
        }

        // Add the subject reference to the structure topic
        structureTopic.AddSubjectReference(subject.Id);

        // Save the structure topic changes
        await knowledgeStructureRepository.UpdateTopicAsync(structureTopic, cancellationToken);

        return Success(subject.Id);
    }
}
