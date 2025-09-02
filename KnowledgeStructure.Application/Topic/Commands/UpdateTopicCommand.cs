using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Commands;

public sealed record UpdateTopicCommand(
    long StructureTopicId,
    string Name,
    string? Description) : IBaseRequest;

public class UpdateTopicCommandValidator : AbstractValidator<UpdateTopicCommand>
{
    public UpdateTopicCommandValidator()
    {
        RuleFor(x => x.StructureTopicId)
            .GreaterThan(0).WithMessage("El identificador del tema es requerido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}

public sealed class UpdateTopicCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository) : BaseCommandHandler<UpdateTopicCommand>
{
    public override async Task<Result> Handle(UpdateTopicCommand request, CancellationToken cancellationToken)
    {
        // Get the structure topic with its topic entity
        var structureTopic = await knowledgeStructureRepository.GetStructureTopicByIdAsync(request.StructureTopicId, cancellationToken);
        if (structureTopic is null || structureTopic.Topic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(request.StructureTopicId), "El tema no existe."));
        }

        // Check if another topic with the same name already exists
        var allTopics = await knowledgeStructureRepository.GetAllStructureTopicsAsync(cancellationToken);
        var existingTopic = allTopics
            .Where(st => st.Topic.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) && st.TopicId != structureTopic.TopicId)
            .FirstOrDefault();

        if (existingTopic is not null)
        {
            return Failure(
                ResultErrorCodes.Topic_NameAlreadyExists,
                (nameof(request.Name), "Ya existe otro tema con el mismo nombre."));
        }

        // Update the topic
        structureTopic.Topic.Update(request.Name, request.Description);

        // Save changes
        await knowledgeStructureRepository.UpdateTopicAsync(structureTopic, cancellationToken);

        return Success();
    }
}
