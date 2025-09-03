using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands;

public sealed record AddModuleToStructureCommand(
    long KnowledgeStructureId,
    long ModuleId,
    int Order) : IBaseRequest;

public sealed class AddModuleToStructureCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    IModuleRepository moduleRepository) : BaseCommandHandler<AddModuleToStructureCommand>
{
    public override async Task<Result> Handle(
        AddModuleToStructureCommand request,
        CancellationToken cancellationToken)
    {
        var knowledgeStructure = await knowledgeStructureRepository.GetWithModulesAndTopicsByIdAsync(
            request.KnowledgeStructureId,
            cancellationToken);

        if (knowledgeStructure is null)
        {
            return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                ("KnowledgeStructureId", "Estructura de conocimiento no encontrada"));
        }

        var module = await moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Failure(ResultErrorCodes.Module_NotFound,
                ("ModuleId", "Módulo no encontrado"));
        }

        // Add module to structure
        knowledgeStructure.AddModule(module, request.Order);

        // Save changes
        await knowledgeStructureRepository.UpdateKnowledgeStructureAsync(knowledgeStructure, cancellationToken);

        return Success();
    }
}