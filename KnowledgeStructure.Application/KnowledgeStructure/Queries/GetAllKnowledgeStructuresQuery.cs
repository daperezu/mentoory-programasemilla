using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;

public sealed record GetAllKnowledgeStructuresQuery : IBaseRequest<List<KnowledgeStructureListDto>>;

public sealed record KnowledgeStructureListDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int ModuleCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class GetAllKnowledgeStructuresQueryHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<GetAllKnowledgeStructuresQuery, List<KnowledgeStructureListDto>>
{
    public override async Task<Result<List<KnowledgeStructureListDto>>> Handle(
        GetAllKnowledgeStructuresQuery request,
        CancellationToken cancellationToken)
    {
        var structures = await repository.ListAllActiveAsync(cancellationToken);

        var result = structures
            .Select(s => new KnowledgeStructureListDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive,
                ModuleCount = s.KnowledgeStructureModules.Count,
                CreatedAt = s.CreatedAt,
            })
            .OrderBy(s => s.Name)
            .ToList();

        return Success(result);
    }
}
