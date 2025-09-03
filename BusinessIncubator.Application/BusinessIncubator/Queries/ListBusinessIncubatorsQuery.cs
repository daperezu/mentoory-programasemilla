using System.Linq.Expressions;
using FluentValidation;
using LinaSys.BusinessIncubator.Infrastructure.Persistence;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Extensions;
using LinaSys.Shared.Application.MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using bzInc = LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.BusinessIncubator;

namespace LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;

/// <summary>
/// Query to list Business Incubators with pagination and filtering options.
/// </summary>
/// <param name="Start">The starting index of the records to retrieve.</param>
/// <param name="Length">The number of records to retrieve.</param>
/// <param name="GlobalSearch">The global search term to filter records.</param>
/// <param name="Name">The name to filter records.</param>
/// <param name="Description">The description to filter records.</param>
/// <param name="Key">The key to filter records.</param>
/// <param name="StatusId">The status ID to filter records.</param>
/// <param name="OrderByColumn">The column to order the records by.</param>
/// <param name="OrderDirection">The direction to order the records (asc or desc).</param>
public record ListBusinessIncubatorsQuery(
    int Start,
    int Length,
    string? GlobalSearch,
    string? Name,
    string? Description,
    string? Key,
    int? StatusId,
    string? OrderByColumn,
    string? OrderDirection) : IBaseRequest<FilteredQueryResult<BusinessIncubatorListItemDto>>;

/// <summary>
/// Data transfer object for Business Incubator list items.
/// </summary>
/// <param name="ExternalId">The external ID of the Business Incubator.</param>
/// <param name="Name">The name of the Business Incubator.</param>
/// <param name="Description">The description of the Business Incubator.</param>
/// <param name="Key">The key of the Business Incubator.</param>
/// <param name="Status">The status of the Business Incubator.</param>
public record BusinessIncubatorListItemDto(Guid ExternalId, string Name, string? Description, string Key, int Status);

/// <summary>
/// Validator for <see cref="ListBusinessIncubatorsQuery"/>.
/// </summary>
public class ListBusinessIncubatorsQueryValidator : AbstractValidator<ListBusinessIncubatorsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListBusinessIncubatorsQueryValidator"/> class.
    /// </summary>
    public ListBusinessIncubatorsQueryValidator()
    {
        RuleFor(x => x.Start).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Length).GreaterThan(0);
    }
}

/// <summary>
/// Handler for <see cref="ListBusinessIncubatorsQuery"/>.
/// </summary>
public partial class ListBusinessIncubatorsQueryHandler(ILogger<ListBusinessIncubatorsQueryHandler> logger, BusinessIncubatorDbContext context)
    : BaseCommandHandler<ListBusinessIncubatorsQuery, FilteredQueryResult<BusinessIncubatorListItemDto>>
{
    /// <summary>
    /// Handles the <see cref="ListBusinessIncubatorsQuery"/>.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the filtered query result.</returns>
    /// <remarks>
    /// The method performs the following steps:
    /// 1. Retrieves the base query from the context.
    /// 2. Counts the total number of records.
    /// 3. Applies filters to the query based on the request parameters.
    /// 4. Counts the total number of filtered records.
    /// 5. Applies ordering and paging to the filtered query.
    /// 6. Projects the query results to the <see cref="BusinessIncubatorListItemDto"/>.
    /// 7. Returns the filtered query result.
    /// </remarks>
    public override async Task<Result<FilteredQueryResult<BusinessIncubatorListItemDto>>> Handle(
        ListBusinessIncubatorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var baseQuery = context.BusinessIncubators.AsNoTracking();

            var total = await baseQuery.CountAsync(cancellationToken);

            var filteredQuery = ApplyFilters(baseQuery, request);

            var totalFiltered = await filteredQuery.CountAsync(cancellationToken);

            var orderingMap = new Dictionary<string, Expression<Func<bzInc, object?>>>
            {
                [nameof(bzInc.Name).ToLower()] = b => b.Name,
                [nameof(bzInc.Description).ToLower()] = b => b.Description,
                [nameof(bzInc.Key).ToLower()] = b => b.Key,
                [nameof(bzInc.Status).ToLower()] = b => EF.Property<int>(b, nameof(bzInc.Status)),
            };

            var paged = filteredQuery
                .ApplyOrdering(request.OrderByColumn, request.OrderDirection, orderingMap)
                .ApplyPaging(request.Start, request.Length);

            var data = await paged
                .Select(b => new BusinessIncubatorListItemDto(
                    b.ExternalId,
                    b.Name,
                    b.Description,
                    b.Key,
                    (int)b.Status))
                .ToListAsync(cancellationToken);

            return Success(FilteredQueryResult.From(data, total, totalFiltered));
        }
        catch (Exception ex)
        {
            LogQueryFailed(ex.Message);
            return Failure(ResultErrorCodes.BusinessIncubator_FilterQueryFailed, (nameof(ListBusinessIncubatorsQuery), ex.Message));
        }
    }

    /// <summary>
    /// Applies filters to the query based on the request parameters.
    /// </summary>
    /// <param name="query">The base query.</param>
    /// <param name="request">The query request.</param>
    /// <returns>The filtered query.</returns>
    public IQueryable<bzInc> ApplyFilters(
        IQueryable<bzInc> query,
        ListBusinessIncubatorsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.GlobalSearch))
        {
            var search = request.GlobalSearch.ToLowerInvariant();
            query = query.Where(b =>
                b.Name.ToLower().Contains(search) ||
                (b.Description != null && b.Description.ToLower().Contains(search)) ||
                b.Key.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(b => b.Name.ToLower().Contains(request.Name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            query = query.Where(b => b.Description != null && b.Description.ToLower().Contains(request.Description.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(request.Key))
        {
            query = query.Where(b => b.Key.ToLower().Contains(request.Key.ToLower()));
        }

        if (request.StatusId is not null)
        {
            query = query.Where(b => EF.Property<int>(b, nameof(bzInc.Status)) == request.StatusId);
        }

        return query;
    }

    /// <summary>
    /// Logs a query failure.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    [LoggerMessage(EventId = 200_060, Level = LogLevel.Error, Message = nameof(ListBusinessIncubatorsQueryHandler) + " failed with error: {ErrorMessage}")]
    partial void LogQueryFailed(string errorMessage);
}
