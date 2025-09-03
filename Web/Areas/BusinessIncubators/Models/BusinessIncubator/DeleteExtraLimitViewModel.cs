using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.BusinessIncubator;

public class DeleteExtraLimitViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public int Type { get; set; }

    [Required]
    public int Quantity { get; set; }
}
