using LinaSys.Shared.Application;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Models;

/// <summary>
/// Provides extension methods for converting <see cref="FilteredQueryResult{T}"/> to JSON response for DataTables.
/// </summary>
public static class DataTableResponse
{
    /// <summary>
    /// Converts a <see cref="FilteredQueryResult{T}"/> to a JSON response for DataTables.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="response">The filtered query result.</param>
    /// <param name="request">The DataTables request.</param>
    /// <returns>A <see cref="JsonResult"/> containing the DataTables response.</returns>
    public static JsonResult ToJson<T>(this FilteredQueryResult<T> response, DataTableRequest request)
    {
        return new JsonResult(new
        {
            draw = request.Draw,
            recordsTotal = response.RecordsTotal,
            recordsFiltered = response.RecordsFiltered,
            data = response.Data,
        });
    }
}

/// <summary>
/// Represents a response to a DataTables request for server-side processing.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
public class DataTableResponse<T>
{
    /// <summary>
    /// Gets the data to be returned to DataTables.
    /// </summary>
    public List<T> Data { get; init; }

    /// <summary>
    /// Gets the draw counter that this object is a response to - from the DataTables request.
    /// </summary>
    public int Draw { get; init; }

    /// <summary>
    /// Gets the number of records after filtering (i.e. the total number of records after filtering has been applied).
    /// </summary>
    public int RecordsFiltered { get; init; }

    /// <summary>
    /// Gets the total number of records in the database (i.e. the total number of records before filtering has been applied).
    /// </summary>
    public int RecordsTotal { get; init; }
}
