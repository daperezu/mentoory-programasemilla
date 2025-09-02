using FluentValidation;
using LinaSys.KnowledgeStructure.Application.Topic.Commands;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Orchestration.Application.KnowledgeStructure.Commands;

public sealed record CreateTopicOrchestrationCommand(
    string Name,
    string? Description,
    long StructureModuleId) : IBaseRequest<long>;

public class CreateTopicOrchestrationCommandValidator : AbstractValidator<CreateTopicOrchestrationCommand>
{
    public CreateTopicOrchestrationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.StructureModuleId)
            .GreaterThan(0).WithMessage("El módulo es requerido.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}

public sealed class CreateTopicOrchestrationCommandHandler(
    IMediator mediator,
    IKnowledgeStructureRepository knowledgeStructureRepository) : BaseCommandHandler<CreateTopicOrchestrationCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateTopicOrchestrationCommand request,
        CancellationToken cancellationToken)
    {
        // Get the structure module
        var structureModule = await knowledgeStructureRepository.GetStructureModuleWithTopicsAsync(
            request.StructureModuleId,
            cancellationToken);

        if (structureModule is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.StructureModuleId), "El módulo no existe."));
        }

        // Create the topic through its own aggregate
        var createTopicCommand = new CreateTopicCommand(request.Name, request.Description);
        var topicResult = await mediator.Send(createTopicCommand, cancellationToken);

        if (!topicResult.IsSuccess)
        {
            return Failure(topicResult.ErrorCode ?? ResultErrorCodes.Unknown, topicResult.ErrorMessages ?? []);
        }

        var topicId = topicResult.Value;

        // Get the topic that was created
        var allTopics = await knowledgeStructureRepository.GetAllStructureTopicsAsync(cancellationToken);
        var topic = allTopics.FirstOrDefault(t => t.TopicId == topicId)?.Topic;

        if (topic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                ("Topic", "El tema creado no fue encontrado."));
        }

        // Add the topic to the structure module
        var order = structureModule.KnowledgeStructureTopics.Count + 1;
        structureModule.AddTopic(topic, order);

        // Save the structure module changes
        await knowledgeStructureRepository.UpdateModuleAsync(structureModule, cancellationToken);

        // Return the structure topic ID (not the topic ID)
        var structureTopic = structureModule.KnowledgeStructureTopics.First(st => st.TopicId == topic.Id);
        return Success(structureTopic.Id);
    }
}
