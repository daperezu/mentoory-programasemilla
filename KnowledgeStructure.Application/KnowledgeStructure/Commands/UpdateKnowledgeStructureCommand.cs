using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands;

public record UpdateKnowledgeStructureCommand(
    long Id,
    string Name,
    string? Description,
    bool IsActive) : IBaseRequest;

public class UpdateKnowledgeStructureCommandValidator : AbstractValidator<UpdateKnowledgeStructureCommand>
{
    public UpdateKnowledgeStructureCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID es requerido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.");
    }
}

public class UpdateKnowledgeStructureCommandHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<UpdateKnowledgeStructureCommand>
{
    public override async Task<Result> Handle(UpdateKnowledgeStructureCommand request, CancellationToken cancellationToken)
    {
        var knowledgeStructure = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (knowledgeStructure is null)
        {
            return Failure(
                ResultErrorCodes.KnowledgeStructure_NotFound,
                (nameof(request.Id), "La estructura de conocimiento no fue encontrada."));
        }

        // Check if name is taken by another knowledge structure
        if (knowledgeStructure.Name != request.Name)
        {
            var isNameTaken = await repository.IsNameTakenAsync(request.Name, request.Id, cancellationToken);
            if (isNameTaken)
            {
                return Failure(
                    ResultErrorCodes.KnowledgeStructure_NameAlreadyExists,
                    (nameof(request.Name), "Ya existe otra estructura de conocimiento con ese nombre."));
            }

            knowledgeStructure.Rename(request.Name);
        }

        // Update description
        knowledgeStructure.UpdateDescription(request.Description);

        // Update active status
        if (!request.IsActive && knowledgeStructure.IsActive)
        {
            knowledgeStructure.Deactivate("Desactivado por el usuario");
        }
        else if (request.IsActive && !knowledgeStructure.IsActive)
        {
            knowledgeStructure.Activate();
        }

        repository.Update(knowledgeStructure);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
