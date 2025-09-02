using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.BusinessIncubator;

public class UpdateSubscriptionViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public int PackageVersionId { get; set; }
}
