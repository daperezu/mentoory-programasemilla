using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Commands;

public sealed record RemoveSubjectFromTopicCommand(
    long StructureTopicId,
    long SubjectId) : IBaseRequest;

public sealed class RemoveSubjectFromTopicCommandValidator : AbstractValidator<RemoveSubjectFromTopicCommand>
{
    public RemoveSubjectFromTopicCommandValidator()
    {
        RuleFor(x => x.StructureTopicId)
            .GreaterThan(0).WithMessage("El ID del tema de estructura debe ser mayor a 0.");

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("El ID de la materia debe ser mayor a 0.");
    }
}

public sealed class RemoveSubjectFromTopicCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<RemoveSubjectFromTopicCommand>
{
    public override async Task<Result> Handle(
        RemoveSubjectFromTopicCommand request,
        CancellationToken cancellationToken)
    {
        // Get the structure topic with its subject references
        var structureTopic = await knowledgeStructureRepository.GetStructureTopicWithSubjectsAsync(
            request.StructureTopicId,
            cancellationToken);

        if (structureTopic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(request.StructureTopicId), "El tema de estructura no existe."));
        }

        // Find the subject reference
        var subjectReference = structureTopic.SubjectReferences
            .FirstOrDefault(sr => sr.SubjectId == request.SubjectId);

        if (subjectReference is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.SubjectId), "La materia no está asignada a este tema."));
        }

        // Remove the subject reference from the topic
        structureTopic.RemoveSubjectReference(request.SubjectId);

        // Save changes
        await knowledgeStructureRepository.UpdateTopicAsync(structureTopic, cancellationToken);

        return Success();
    }
}
