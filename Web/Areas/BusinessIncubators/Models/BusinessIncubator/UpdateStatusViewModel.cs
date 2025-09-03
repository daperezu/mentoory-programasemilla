using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.BusinessIncubator;

public class UpdateStatusViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public int Status { get; set; }
}
