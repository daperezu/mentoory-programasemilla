using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Commands;

public sealed record DeleteTopicCommand(long StructureTopicId) : IBaseRequest;

public sealed class DeleteTopicCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository) : BaseCommandHandler<DeleteTopicCommand>
{
    public override async Task<Result> Handle(DeleteTopicCommand request, CancellationToken cancellationToken)
    {
        // Get the structure topic with its subjects
        var structureTopic = await knowledgeStructureRepository.GetStructureTopicWithSubjectsAsync(request.StructureTopicId, cancellationToken);
        if (structureTopic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(request.StructureTopicId), "El tema no existe."));
        }

        // Check if topic has subjects
        if (structureTopic.SubjectReferences.Any())
        {
            return Failure(
                ResultErrorCodes.Topic_CannotDeleteWithSubjects,
                (nameof(request.StructureTopicId), "No se puede eliminar un tema que tiene asignaturas."));
        }

        // Remove the topic from the structure module
        structureTopic.KnowledgeStructureModule.RemoveTopic(structureTopic.Id);

        // Save changes
        await knowledgeStructureRepository.UpdateModuleAsync(structureTopic.KnowledgeStructureModule, cancellationToken);

        return Success();
    }
}
