using FluentValidation;
using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands;
using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;
using LinaSys.KnowledgeStructure.Application.Module.Commands;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Orchestration.Application.KnowledgeStructure.Commands;

public record CreateModuleOrchestrationCommand(string Name, long KnowledgeStructureId) : IBaseRequest<long>;

public class CreateModuleOrchestrationCommandValidator : AbstractValidator<CreateModuleOrchestrationCommand>
{
    public CreateModuleOrchestrationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.KnowledgeStructureId)
            .GreaterThan(0).WithMessage("La estructura de conocimiento es requerida.");
    }
}

public class CreateModuleOrchestrationCommandHandler(
    IMediator mediator)
    : BaseCommandHandler<CreateModuleOrchestrationCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateModuleOrchestrationCommand request,
        CancellationToken cancellationToken)
    {
        // Verify the knowledge structure exists
        var structureQuery = new GetKnowledgeStructureWithModulesQuery(request.KnowledgeStructureId);
        var structureResult = await mediator.Send(structureQuery, cancellationToken);

        if (!structureResult.IsSuccess)
        {
            return Failure(
                ResultErrorCodes.KnowledgeStructure_NotFound,
                (nameof(request.KnowledgeStructureId), "La estructura de conocimiento no existe."));
        }

        var knowledgeStructure = structureResult.Value!;

        // Create the module through its own aggregate
        var createModuleCommand = new CreateModuleCommand(request.Name);
        var moduleResult = await mediator.Send(createModuleCommand, cancellationToken);

        if (!moduleResult.IsSuccess)
        {
            return Failure(moduleResult.ErrorCode ?? ResultErrorCodes.Unknown, moduleResult.ErrorMessages ?? []);
        }

        var moduleId = moduleResult.Value;

        // Add the module to the knowledge structure
        var order = knowledgeStructure.Modules.Count + 1;
        var addModuleCommand = new AddModuleToStructureCommand(
            request.KnowledgeStructureId,
            moduleId,
            order);
        var addResult = await mediator.Send(addModuleCommand, cancellationToken);
        if (!addResult.IsSuccess)
        {
            return Failure(
                addResult.ErrorCode ?? ResultErrorCodes.Unknown,
                addResult.ErrorMessages ?? []);
        }

        return Success(moduleId);
    }
}
