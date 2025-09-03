using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Commands;

public sealed record MoveTopicCommand(long TopicId, long ModuleId, int NewPosition) : IBaseRequest;

public sealed class MoveTopicCommandHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<MoveTopicCommand>
{
    public override async Task<Result> Handle(MoveTopicCommand request, CancellationToken cancellationToken)
    {
        var module = await repository.GetModuleWithTopicsAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                ("Module", $"Módulo con ID {request.ModuleId} no encontrado"));
        }

        var topic = module.KnowledgeStructureTopics.FirstOrDefault(t => t.Id == request.TopicId);
        if (topic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                ("Topic", $"Tema con ID {request.TopicId} no encontrado"));
        }

        // Reorder topics
        var topics = module.KnowledgeStructureTopics.OrderBy(t => t.Order).ToList();
        topics.Remove(topic);

        // Ensure the position is within bounds
        var insertPosition = Math.Max(0, Math.Min(request.NewPosition, topics.Count));
        topics.Insert(insertPosition, topic);

        // Update order for all topics
        for (int i = 0; i < topics.Count; i++)
        {
            topics[i].Reorder(i + 1);
        }

        await repository.UpdateModuleAsync(module, cancellationToken);

        return Success();
    }
}
