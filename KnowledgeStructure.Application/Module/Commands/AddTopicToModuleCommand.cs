using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Commands;

public sealed record AddTopicToModuleCommand(
    long StructureModuleId,
    long TopicId,
    int? Order = null) : IBaseRequest;

public sealed class AddTopicToModuleCommandValidator : AbstractValidator<AddTopicToModuleCommand>
{
    public AddTopicToModuleCommandValidator()
    {
        RuleFor(x => x.StructureModuleId)
            .GreaterThan(0).WithMessage("El ID del módulo de estructura debe ser mayor a 0.");

        RuleFor(x => x.TopicId)
            .GreaterThan(0).WithMessage("El ID del tema debe ser mayor a 0.");

        RuleFor(x => x.Order)
            .GreaterThan(0).When(x => x.Order.HasValue)
            .WithMessage("El orden debe ser mayor a 0.");
    }
}

public sealed class AddTopicToModuleCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ITopicRepository topicRepository)
    : BaseCommandHandler<AddTopicToModuleCommand>
{
    public override async Task<Result> Handle(
        AddTopicToModuleCommand request,
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

        // Get the topic
        var topic = await topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
        if (topic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(request.TopicId), "El tema no existe."));
        }

        // Check if topic is already in this module
        var existingTopic = structureModule.KnowledgeStructureTopics
            .FirstOrDefault(t => t.TopicId == request.TopicId);

        if (existingTopic is not null)
        {
            return Failure(
                ResultErrorCodes.Topic_AlreadyExists,
                (nameof(request.TopicId), "El tema ya está asignado a este módulo."));
        }

        // Determine the order
        var order = request.Order ?? (structureModule.KnowledgeStructureTopics.Count + 1);

        // Add the topic to the module
        structureModule.AddTopic(topic, order);

        // Save changes
        await knowledgeStructureRepository.UpdateModuleAsync(structureModule, cancellationToken);

        return Success();
    }
}
