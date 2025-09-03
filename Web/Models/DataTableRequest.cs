namespace LinaSys.Web.Models;

/// <summary>
/// Represents a request from DataTables for server-side processing.
/// </summary>
public class DataTableRequest
{
    /// <summary>
    /// Gets the column-specific search values.
    /// </summary>
    public Dictionary<string, string?> ColumnSearches { get; } = [];

    /// <summary>
    /// Gets the draw counter that this object is a response to - from the DataTables request.
    /// </summary>
    public int Draw { get; init; }

    /// <summary>
    /// Gets the global search value to be applied to all columns.
    /// </summary>
    public string? GlobalSearch { get; init; }

    /// <summary>
    /// Gets the number of records that the table can display in the current draw.
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// Gets the column to which ordering should be applied.
    /// </summary>
    public string? OrderByColumn { get; init; }

    /// <summary>
    /// Gets the direction of ordering (ascending/descending).
    /// </summary>
    public string? OrderDirection { get; init; }

    /// <summary>
    /// Gets the first record that should be shown (used for paging).
    /// </summary>
    public int Start { get; init; }
}
