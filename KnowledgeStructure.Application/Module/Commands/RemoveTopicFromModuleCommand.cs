using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Commands;

public sealed record RemoveTopicFromModuleCommand(
    long StructureModuleId,
    long StructureTopicId) : IBaseRequest;

public sealed class RemoveTopicFromModuleCommandValidator : AbstractValidator<RemoveTopicFromModuleCommand>
{
    public RemoveTopicFromModuleCommandValidator()
    {
        RuleFor(x => x.StructureModuleId)
            .GreaterThan(0).WithMessage("El ID del módulo de estructura debe ser mayor a 0.");

        RuleFor(x => x.StructureTopicId)
            .GreaterThan(0).WithMessage("El ID del tema de estructura debe ser mayor a 0.");
    }
}

public sealed class RemoveTopicFromModuleCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<RemoveTopicFromModuleCommand>
{
    public override async Task<Result> Handle(
        RemoveTopicFromModuleCommand request,
        CancellationToken cancellationToken)
    {
        // Get the structure module with its topics
        var structureModule = await knowledgeStructureRepository.GetStructureModuleWithTopicsAsync(
            request.StructureModuleId,
            cancellationToken);

        if (structureModule is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.StructureModuleId), "El módulo de estructura no existe."));
        }

        // Find the structure topic
        var structureTopic = structureModule.KnowledgeStructureTopics
            .FirstOrDefault(t => t.Id == request.StructureTopicId);

        if (structureTopic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(request.StructureTopicId), "El tema no está asignado a este módulo."));
        }

        // Check if the topic has subject references - prevent deletion if it has content
        var topicWithRefs = await knowledgeStructureRepository.GetStructureTopicWithSubjectsAsync(
            request.StructureTopicId,
            cancellationToken);

        if (topicWithRefs?.SubjectReferences.Any() == true)
        {
            return Failure(
                ResultErrorCodes.Topic_HasDependencies,
                (nameof(request.StructureTopicId), "No se puede eliminar el tema porque tiene materias asociadas."));
        }

        // Remove the topic from the module
        structureModule.RemoveTopic(request.StructureTopicId);

        // Save changes
        await knowledgeStructureRepository.UpdateModuleAsync(structureModule, cancellationToken);

        return Success();
    }
}
