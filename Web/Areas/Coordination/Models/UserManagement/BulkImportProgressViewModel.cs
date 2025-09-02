using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

/// <summary>
/// View model for displaying bulk import progress.
/// </summary>
public class BulkImportProgressViewModel
{
    /// <summary>
    /// Gets or sets the operation ID for tracking.
    /// </summary>
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of items to process.
    /// </summary>
    public int TotalItems { get; set; }
}
