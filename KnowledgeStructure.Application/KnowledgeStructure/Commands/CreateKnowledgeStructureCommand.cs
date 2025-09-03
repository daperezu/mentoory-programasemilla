using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands;

public record CreateKnowledgeStructureCommand(
    string Name,
    string? Description,
    bool IsActive = true) : IBaseRequest<long>;

public class CreateKnowledgeStructureCommandValidator : AbstractValidator<CreateKnowledgeStructureCommand>
{
    public CreateKnowledgeStructureCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.");
    }
}

public class CreateKnowledgeStructureCommandHandler(IKnowledgeStructureRepository repository, ITimeProvider timeProvider)
    : BaseCommandHandler<CreateKnowledgeStructureCommand, long>
{
    public override async Task<Result<long>> Handle(CreateKnowledgeStructureCommand request, CancellationToken cancellationToken)
    {
        // Check if a knowledge structure with the same name already exists
        var isNameTaken = await repository.IsNameTakenAsync(request.Name, cancellationToken: cancellationToken);
        if (isNameTaken)
        {
            return Failure(
                ResultErrorCodes.KnowledgeStructure_NameAlreadyExists,
                (nameof(request.Name), "Ya existe una estructura de conocimiento con ese nombre."));
        }

        var knowledgeStructure = new Domain.Aggregates.KnowledgeStructure.KnowledgeStructure(
            request.Name,
            request.Description,
            request.IsActive,
            timeProvider.UtcNow);

        repository.Add(knowledgeStructure);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(knowledgeStructure.Id);
    }
}
