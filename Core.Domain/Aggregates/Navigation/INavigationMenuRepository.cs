using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Navigation
{
    public interface INavigationMenuRepository : IRepository<NavigationMenuItem>
    {
        Task<IEnumerable<NavigationMenuItem>> GetActiveMenuTreeAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<NavigationMenuItem>> GetMenuItemsByRolesAsync(
            string[] roles,
            CancellationToken cancellationToken = default);

        Task<NavigationMenuItem?> GetMenuItemWithChildrenAsync(
            long id,
            CancellationToken cancellationToken = default);

        Task<NavigationMenuItem?> GetMenuItemByCodeAsync(
            string code,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<NavigationMenuItem>> GetRootMenuItemsAsync(
            CancellationToken cancellationToken = default);

        Task<bool> CodeExistsAsync(
            string code,
            long? excludeId = null,
            CancellationToken cancellationToken = default);
    }
}