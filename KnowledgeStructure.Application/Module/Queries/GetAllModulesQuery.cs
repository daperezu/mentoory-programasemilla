using LinaSys.KnowledgeStructure.Application.Module.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record GetAllModulesQuery(long? KnowledgeStructureId = null) : IBaseRequest<List<ModuleDto>>;

public sealed class GetAllModulesQueryHandler(
    IModuleRepository repository) : BaseCommandHandler<GetAllModulesQuery, List<ModuleDto>>
{
    public override async Task<Result<List<ModuleDto>>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken)
    {
        // Use repository method instead of IQueryable
        var modules = await repository.ListAllAsync(cancellationToken);

        // Convert to DTOs
        var moduleDtos = modules
            .OrderBy(m => m.Name)
            .Select(m => new ModuleDto(
                m.Id,
                m.Name,
                string.Empty, // Description not in domain
                0, // KnowledgeStructureId not directly accessible
                string.Empty, // KnowledgeStructure.Name not directly accessible
                0)) // Order not directly accessible
            .ToList();

        return Success(moduleDtos);
    }
}
