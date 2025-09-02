namespace LinaSys.SystemFeatures.Domain.AggregatesModel.WebFeatureAggregate;

public interface IWebFeatureRepository
{
    Task<WebFeature?> GetByAreaControllerAndActionAsync(string area, string controller, string action, CancellationToken cancellationToken);

    Task<IEnumerable<WebFeature>> GetActiveMenuItemsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<WebFeature>> GetMenuHierarchyAsync(CancellationToken cancellationToken);

    Task<WebFeature?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<IEnumerable<WebFeature>> GetByParentIdAsync(long? parentId, CancellationToken cancellationToken);
}
