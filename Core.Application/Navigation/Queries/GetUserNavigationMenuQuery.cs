using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinaSys.Core.Domain.Aggregates.Navigation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Navigation.Queries
{
    public record GetUserNavigationMenuQuery(
        string UserId,
        string? RoleName,
        long? IncubatorId,
        long? ProjectId) : IBaseRequest<NavigationMenuDto>;

    public record NavigationMenuDto(
        List<NavigationMenuItemDto> Items,
        NavigationContextDto Context);

    public record NavigationMenuItemDto(
        long Id,
        string Code,
        string DisplayText,
        string? Icon,
        string Url,
        string? CssClass,
        bool IsSection,
        List<NavigationMenuItemDto> Children);

    public record NavigationContextDto(
        string UserId,
        string? RoleName,
        long? IncubatorId,
        long? ProjectId);

    public class GetUserNavigationMenuQueryHandler(INavigationMenuRepository repository) : BaseCommandHandler<GetUserNavigationMenuQuery, NavigationMenuDto>
    {
        public override async Task<Result<NavigationMenuDto>> Handle(
            GetUserNavigationMenuQuery request,
            CancellationToken cancellationToken)
        {
            // Get all active menu items
            var menuItems = await repository.GetActiveMenuTreeAsync(cancellationToken);

            // Filter by context and role
            var filteredItems = menuItems
                .Where(item => item.IsVisibleInContext(
                    request.IncubatorId.HasValue,
                    request.ProjectId.HasValue,
                    !string.IsNullOrEmpty(request.UserId)))
                .Where(item => string.IsNullOrEmpty(request.RoleName) ||
                              item.IsAllowedForRole(request.RoleName))
                .ToList();

            // Build hierarchy
            var menuTree = BuildMenuTree(filteredItems);

            // Create context
            var context = new NavigationContextDto(
                request.UserId,
                request.RoleName,
                request.IncubatorId,
                request.ProjectId);

            return Success(new NavigationMenuDto(menuTree, context));
        }

        private List<NavigationMenuItemDto> BuildMenuTree(
            IEnumerable<NavigationMenuItem> items,
            long? parentId = null)
        {
            return items
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.SortOrder)
                .Select(item => new NavigationMenuItemDto(
                    item.Id,
                    item.Code,
                    item.DisplayText,
                    item.Icon,
                    item.GetUrl(),
                    item.CssClass,
                    item.IsSection,
                    BuildMenuTree(items, item.Id)))
                .ToList();
        }
    }
}