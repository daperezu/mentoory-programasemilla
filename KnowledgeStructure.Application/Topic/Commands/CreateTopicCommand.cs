using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Commands;

public sealed record CreateTopicCommand(
    string Name,
    string? Description) : IBaseRequest<long>;

public class CreateTopicCommandValidator : AbstractValidator<CreateTopicCommand>
{
    public CreateTopicCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}

public sealed class CreateTopicCommandHandler(ITopicRepository topicRepository)
    : BaseCommandHandler<CreateTopicCommand, long>
{
    public override async Task<Result<long>> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
    {
        // Check if a topic with the same name already exists
        var exists = await topicRepository.IsNameTakenAsync(request.Name, cancellationToken: cancellationToken);
        if (exists)
        {
            return Failure(
                ResultErrorCodes.Topic_NameAlreadyExists,
                (nameof(request.Name), "Ya existe un tema con el mismo nombre."));
        }

        // Create the topic
        var topic = new Domain.Aggregates.Topic.Topic(request.Name);
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            topic.Update(request.Name, request.Description);
        }

        topicRepository.Add(topic);

        // Save to get the topic ID
        await topicRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(topic.Id);
    }
}
