using System.Linq.Expressions;
using FluentValidation;
using LinaSys.Diagnostics.Infrastructure.Persistence;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Extensions;
using LinaSys.Shared.Application.MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using bzForm = LinaSys.Diagnostics.Domain.Aggregates.Form.Form;

namespace LinaSys.Diagnostics.Application.Form.Queries;

public record ListFormsQuery(
    int Start,
    int Length,
    string? Name,
    string? OrderByColumn,
    string? OrderDirection) : IBaseRequest<FilteredQueryResult<FormListItemDto>>;

public record FormListItemDto(long Id, string Name);

public class ListFormsQueryValidator : AbstractValidator<ListFormsQuery>
{
    public ListFormsQueryValidator()
    {
        RuleFor(x => x.Start).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Length).GreaterThan(0);
    }
}

public partial class ListFormsQueryHandler(ILogger<ListFormsQueryHandler> logger, DiagnosticsDbContext context)
    : BaseCommandHandler<ListFormsQuery, FilteredQueryResult<FormListItemDto>>
{
    public IQueryable<bzForm> ApplyFilters(
        IQueryable<bzForm> query,
        ListFormsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(b => b.Name.ToLower().Contains(request.Name.ToLower()));
        }

        return query;
    }

    public override async Task<Result<FilteredQueryResult<FormListItemDto>>> Handle(ListFormsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var baseQuery = context.Forms.AsNoTracking();

            var total = await baseQuery.CountAsync(cancellationToken);

            var filteredQuery = ApplyFilters(baseQuery, request);

            var totalFiltered = await filteredQuery.CountAsync(cancellationToken);

            var orderingMap = new Dictionary<string, Expression<Func<bzForm, object?>>>
            {
                [nameof(bzForm.Name).ToLower()] = b => b.Name,
            };

            var paged = filteredQuery
                .ApplyOrdering(request.OrderByColumn, request.OrderDirection, orderingMap)
                .ApplyPaging(request.Start, request.Length);

            var data = await paged
                .Select(b => new FormListItemDto(
                    b.Id,
                    b.Name))
                .ToListAsync(cancellationToken);

            return Success(FilteredQueryResult.From(data, total, totalFiltered));
        }
        catch (Exception ex)
        {
            LogQueryFailed(ex.Message);
            return Failure(ResultErrorCodes.BusinessIncubator_FilterQueryFailed, (nameof(ListFormsQuery), ex.Message));
        }
    }

    [LoggerMessage(EventId = 800_090, Level = LogLevel.Error, Message = nameof(ListFormsQueryHandler) + " failed with error: {ErrorMessage}")]
    partial void LogQueryFailed(string errorMessage);
}
