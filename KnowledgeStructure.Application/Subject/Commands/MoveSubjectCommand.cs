using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Commands;

public sealed record MoveSubjectCommand(long SubjectId, long TopicId, int NewPosition) : IBaseRequest;

public sealed class MoveSubjectCommandHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<MoveSubjectCommand>
{
    public override async Task<Result> Handle(MoveSubjectCommand request, CancellationToken cancellationToken)
    {
        var topic = await repository.GetTopicWithSubjectReferencesAsync(request.TopicId, cancellationToken);
        if (topic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                ("Topic", $"Tema con ID {request.TopicId} no encontrado"));
        }

        var subjectRef = topic.SubjectReferences.FirstOrDefault(sr => sr.SubjectId == request.SubjectId);
        if (subjectRef is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                ("Subject", $"Asignatura con ID {request.SubjectId} no está asociada al tema"));
        }

        // Remove and re-add subject references in new order
        topic.RemoveSubjectReference(request.SubjectId);

        // Get ordered list and insert at new position
        var orderedRefs = topic.SubjectReferences.OrderBy(sr => sr.Order).ToList();
        var reorderedSubjectIds = orderedRefs.Select(sr => sr.SubjectId).ToList();

        // Ensure the position is within bounds
        var insertPosition = Math.Max(0, Math.Min(request.NewPosition, reorderedSubjectIds.Count));
        reorderedSubjectIds.Insert(insertPosition, request.SubjectId);

        // Clear and re-add all references in new order
        foreach (var subjectId in orderedRefs.Select(sr => sr.SubjectId))
        {
            topic.RemoveSubjectReference(subjectId);
        }

        foreach (var subjectId in reorderedSubjectIds)
        {
            topic.AddSubjectReference(subjectId);
        }

        await repository.UpdateTopicAsync(topic, cancellationToken);

        return Success();
    }
}
