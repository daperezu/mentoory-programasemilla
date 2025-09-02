using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record ListModulesQuery(
    int Start,
    int Length,
    string? GlobalSearch,
    string? Name,
    long? KnowledgeStructureId,
    string? OrderByColumn,
    string? OrderDirection) : IBaseRequest<FilteredQueryResult<ModuleListDto>>;

public sealed record ModuleListDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public long KnowledgeStructureId { get; set; }

    public string KnowledgeStructureName { get; set; } = string.Empty;

    public int Order { get; set; }

    public int TopicCount { get; set; }
}

public sealed class ListModulesQueryHandler(IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<ListModulesQuery, FilteredQueryResult<ModuleListDto>>
{
    public override async Task<Result<FilteredQueryResult<ModuleListDto>>> Handle(
        ListModulesQuery request,
        CancellationToken cancellationToken)
    {
        // Get all knowledge structures with their modules
        var knowledgeStructures = await knowledgeStructureRepository.GetAllWithModulesAsync(cancellationToken);

        // Create DTOs from the knowledge structure modules
        var modulesDtos = from ks in knowledgeStructures
                         from ksm in ks.KnowledgeStructureModules
                         select new ModuleListDto
                         {
                             Id = ksm.Module.Id,
                             Name = ksm.Module.Name,
                             KnowledgeStructureId = ks.Id,
                             KnowledgeStructureName = ks.Name,
                             Order = ksm.Order,
                             TopicCount = 0, // TODO: Add topic count if needed
                         };

        var query = modulesDtos.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.GlobalSearch))
        {
            var search = request.GlobalSearch.ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(search) ||
                m.KnowledgeStructureName.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(m => m.Name.ToLower().Contains(request.Name.ToLower()));
        }

        if (request.KnowledgeStructureId.HasValue)
        {
            query = query.Where(m => m.KnowledgeStructureId == request.KnowledgeStructureId.Value);
        }

        // Get total count before pagination
        var totalRecords = query.Count();

        // Apply ordering
        var isDescending = request.OrderDirection?.ToLower() == "desc";
        query = request.OrderByColumn?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name),
            "knowledgestructurename" => isDescending ? query.OrderByDescending(m => m.KnowledgeStructureName) : query.OrderBy(m => m.KnowledgeStructureName),
            "order" => isDescending ? query.OrderByDescending(m => m.Order) : query.OrderBy(m => m.Order),
            "topiccount" => isDescending ? query.OrderByDescending(m => m.TopicCount) : query.OrderBy(m => m.TopicCount),
            _ => query.OrderBy(m => m.Order).ThenBy(m => m.Name),
        };

        // Apply pagination
        var pagedData = query
            .Skip(request.Start)
            .Take(request.Length)
            .ToList();

        var result = new FilteredQueryResult<ModuleListDto>(
            totalRecords,
            totalRecords,
            pagedData);

        return Success(result);
    }
}
