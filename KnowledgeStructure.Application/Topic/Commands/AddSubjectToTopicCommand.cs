using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Commands;

public sealed record AddSubjectToTopicCommand(
    long StructureTopicId,
    long SubjectId,
    int? Order = null) : IBaseRequest;

public sealed class AddSubjectToTopicCommandValidator : AbstractValidator<AddSubjectToTopicCommand>
{
    public AddSubjectToTopicCommandValidator()
    {
        RuleFor(x => x.StructureTopicId)
            .GreaterThan(0).WithMessage("El ID del tema de estructura debe ser mayor a 0.");

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("El ID de la materia debe ser mayor a 0.");

        RuleFor(x => x.Order)
            .GreaterThan(0).When(x => x.Order.HasValue)
            .WithMessage("El orden debe ser mayor a 0.");
    }
}

public sealed class AddSubjectToTopicCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ISubjectRepository subjectRepository)
    : BaseCommandHandler<AddSubjectToTopicCommand>
{
    public override async Task<Result> Handle(
        AddSubjectToTopicCommand request,
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

        // Get the subject
        var subject = await subjectRepository.FindByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.SubjectId), "La materia no existe."));
        }

        // Check if subject is already referenced by this topic
        var existingReference = structureTopic.SubjectReferences
            .FirstOrDefault(sr => sr.SubjectId == request.SubjectId);

        if (existingReference is not null)
        {
            return Failure(
                ResultErrorCodes.Subject_AlreadyExists,
                (nameof(request.SubjectId), "La materia ya está asignada a este tema."));
        }

        // Determine the order
        var order = request.Order ?? (structureTopic.SubjectReferences.Count + 1);

        // Add the subject reference to the topic
        structureTopic.AddSubjectReference(subject.Id, order);

        // Save changes
        await knowledgeStructureRepository.UpdateTopicAsync(structureTopic, cancellationToken);

        return Success();
    }
}
