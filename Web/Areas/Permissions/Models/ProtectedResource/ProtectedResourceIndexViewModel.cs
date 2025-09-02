using LinaSys.Permissions.Domain.Constants;

namespace LinaSys.Web.Areas.Permissions.Models.ProtectedResource;

public class ProtectedResourceIndexViewModel
{
    public static Dictionary<int, string> ResourceTypeOptions { get; } = new()
    {
        { ResourceTypes.WebFeature, ResourceTypes.GetDisplayName(ResourceTypes.WebFeature) },
        { ResourceTypes.BusinessIncubator, ResourceTypes.GetDisplayName(ResourceTypes.BusinessIncubator) },
        { ResourceTypes.Project, ResourceTypes.GetDisplayName(ResourceTypes.Project) },
        { ResourceTypes.DiagnosisForm, ResourceTypes.GetDisplayName(ResourceTypes.DiagnosisForm) },
    };

    public int? ResourceType { get; set; }

    public string? SearchTerm { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public IEnumerable<ProtectedResourceItemViewModel> Resources { get; set; } = [];

    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ProtectedResourceItemViewModel
{
    public long Id { get; set; }

    public Guid ExternalId { get; set; }

    public int ResourceType { get; set; }

    public string ResourceTypeName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public int UserPermissionsCount { get; set; }

    public int RolePermissionsCount { get; set; }
}
