using LinaSys.Subscription.Application.BusinessIncubatorPackage.Queries;
using LinaSys.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.BusinessIncubator;

public class EditViewModel : RestorableViewModel
{
    public const string DetailsTabId = "detailsTabContent";
    public const string StatusTabId = "statusTabContent";
    public const string SubscriptionTabId = "subscriptionTabContent";

    public string? Description { get; set; }

    public Guid Id { get; set; }

    public string Key { get; set; }

    public string Name { get; set; }

    public List<SelectListItem> PackageOptions { get; set; } = [];

    public long? PackageVersionId { get; set; }

    public int Status { get; set; }

    public List<SelectListItem> StatusOptions { get; set; } = [];

    public List<PackageLimit> PackageLimits { get; set; } = [];

    public List<PackageLimit> LimitOverrides { get; set; } = [];

    public List<PackageLimit> EffectiveLimits { get; set; } = [];

    public List<SelectListItem> PackageLimitTypes { get; set; }
}
