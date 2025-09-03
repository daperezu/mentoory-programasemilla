using System.Linq.Expressions;
using FluentValidation;
using LinaSys.BusinessIncubator.Infrastructure.Persistence;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Extensions;
using LinaSys.Shared.Application.MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Proj = LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project;

namespace LinaSys.BusinessIncubator.Application.Project.Queries;

public record ListProjectsQuery(
    int Start,
    int Length,
    Guid BusinessIncubatorExternalId,
    string? Name,
    string? Description,
    string? Key,
    int? StatusId,
    string? OrderByColumn,
    string? OrderDirection) : IBaseRequest<FilteredQueryResult<ProjectListItemDto>>;

public record ProjectListItemDto(Guid ExternalId, string Name, string? Description, string Key, int Status);

public class ListProjectsQueryValidator : AbstractValidator<ListProjectsQuery>
{
    public ListProjectsQueryValidator()
    {
        RuleFor(x => x.Start).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Length).GreaterThan(0);
    }
}

/// <summary>
/// Handler for <see cref="ListProjectsQuery"/>.
/// </summary>
public partial class ListProjectsQueryHandler(ILogger<ListProjectsQueryHandler> logger, BusinessIncubatorDbContext context)
    : BaseCommandHandler<ListProjectsQuery, FilteredQueryResult<ProjectListItemDto>>
{
    public override async Task<Result<FilteredQueryResult<ProjectListItemDto>>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var businessIncubatorId = await context.BusinessIncubators
                .AsNoTracking()
                .Where(w => w.ExternalId == request.BusinessIncubatorExternalId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var baseQuery = context.Projects.AsNoTracking()
                .Where(w => w.BusinessIncubatorId == businessIncubatorId);

            var total = await baseQuery.CountAsync(cancellationToken);

            var filteredQuery = ApplyFilters(baseQuery, request);

            var totalFiltered = await filteredQuery.CountAsync(cancellationToken);

            var orderingMap = new Dictionary<string, Expression<Func<Proj, object?>>>
            {
                [nameof(Proj.Name).ToLower()] = b => b.Name,
                [nameof(Proj.Description).ToLower()] = b => b.Description,
                [nameof(Proj.Key).ToLower()] = b => b.Key,
                [nameof(Proj.Status).ToLower()] = b => EF.Property<int>(b, nameof(Proj.Status)),
            };

            var paged = filteredQuery
                .ApplyOrdering(request.OrderByColumn, request.OrderDirection, orderingMap)
                .ApplyPaging(request.Start, request.Length);

            var data = await paged
                .Select(b => new ProjectListItemDto(
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
            return Failure(ResultErrorCodes.BusinessIncubator_FilterQueryFailed, (nameof(ListProjectsQuery), ex.Message));
        }
    }

    /// <summary>
    /// Applies filters to the query based on the request parameters.
    /// </summary>
    /// <param name="query">The base query.</param>
    /// <param name="request">The query request.</param>
    /// <returns>The filtered query.</returns>
    public IQueryable<Proj> ApplyFilters(IQueryable<Proj> query, ListProjectsQuery request)
    {
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
            query = query.Where(b => EF.Property<int>(b, nameof(Proj.Status)) == request.StatusId);
        }

        return query;
    }

    /// <summary>
    /// Logs a query failure.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    [LoggerMessage(EventId = 200_060, Level = LogLevel.Error, Message = nameof(ListProjectsQueryHandler) + " failed with error: {ErrorMessage}")]
    partial void LogQueryFailed(string errorMessage);
}
