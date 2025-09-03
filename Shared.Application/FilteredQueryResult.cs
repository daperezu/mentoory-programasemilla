namespace LinaSys.Shared.Application;

/// <summary>
/// Data transfer object for DataTable response.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
/// <param name="RecordsTotal">The total number of records.</param>
/// <param name="RecordsFiltered">The number of filtered records.</param>
/// <param name="Data">The list of data items.</param>
public record FilteredQueryResult<T>(int RecordsTotal, int RecordsFiltered, List<T> Data);

/// <summary>
/// Provides factory methods for creating <see cref="FilteredQueryResult{T}"/> instances.
/// </summary>
public static class FilteredQueryResult
{
    /// <summary>
    /// Creates a new <see cref="FilteredQueryResult{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="data">The list of data items.</param>
    /// <param name="total">The total number of records.</param>
    /// <param name="filtered">The number of filtered records.</param>
    /// <returns>A new <see cref="FilteredQueryResult{T}"/> instance.</returns>
    public static FilteredQueryResult<T> From<T>(List<T> data, int total, int filtered) => new(total, filtered, data);
}
