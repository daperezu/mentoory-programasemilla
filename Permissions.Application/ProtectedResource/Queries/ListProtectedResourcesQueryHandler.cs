using LinaSys.Permissions.Domain.Constants;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Application.ProtectedResource.Queries;

public sealed class ListProtectedResourcesQueryHandler(
    IProtectedResourceRepository protectedResourceRepository) : BaseCommandHandler<ListProtectedResourcesQuery, FilteredQueryResult<ListProtectedResourceDto>>
{
    public override async Task<Result<FilteredQueryResult<ListProtectedResourceDto>>> Handle(ListProtectedResourcesQuery request, CancellationToken cancellationToken)
    {
        var resourceTypeFilter = int.TryParse(request.ResourceType, out var resourceTypeId) ? resourceTypeId : (int?)null;

        var (resources, totalCount) = await protectedResourceRepository.ListProtectedResourcesAsync(
            resourceTypeFilter,
            request.GlobalSearch ?? request.Name,
            request.Start,
            request.Length,
            request.OrderByColumn,
            request.OrderDirection,
            cancellationToken);

        var resourceDtos = resources.Select(r => new ListProtectedResourceDto(
            r.Id,
            r.ExternalId,
            r.ResourceType,
            ResourceTypes.GetDisplayName(r.ResourceType),
            r.Name,
            r.CreatedAt,
            r.CreatedBy ?? string.Empty,
            r.UserProtectedResourcePermissions.Count,
            r.RoleProtectedResourcePermissions.Count)).ToList();

        var result = FilteredQueryResult.From(resourceDtos, totalCount, totalCount);
        return Success(result);
    }
}
