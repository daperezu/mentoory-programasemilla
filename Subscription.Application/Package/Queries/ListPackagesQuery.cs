using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Subscription.Application.Package.Queries;

public record ListPackagesQuery(
    int Start,
    int Length,
    string? Name,
    string? OrderByColumn,
    string? OrderDirection) : IBaseRequest<FilteredQueryResult<PackageListItemDto>>;

public record PackageListItemDto(long Id, string Name, int VersionCount);

public class ListPackagesQueryValidator : AbstractValidator<ListPackagesQuery>
{
    public ListPackagesQueryValidator()
    {
        RuleFor(x => x.Start).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Length).GreaterThan(0);
    }
}

public class ListPackagesQueryHandler(SubscriptionDbContext dbContext)
    : BaseCommandHandler<ListPackagesQuery, FilteredQueryResult<PackageListItemDto>>
{
    public override async Task<Result<FilteredQueryResult<PackageListItemDto>>> Handle(ListPackagesQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Packages
            .AsNoTracking()
            .Include(p => p.PackageVersions)
            .AsQueryable();

        var recordsTotal = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));
        }

        var recordsFiltered = await query.CountAsync(cancellationToken);

        query = (request.OrderByColumn?.ToLowerInvariant(), request.OrderDirection?.ToLowerInvariant()) switch
        {
            ("name", "desc") => query.OrderByDescending(p => p.Name),
            ("name", _) => query.OrderBy(p => p.Name),
            _ => query.OrderBy(p => p.Name),
        };

        var data = await query
            .Skip(request.Start)
            .Take(request.Length)
            .Select(p => new PackageListItemDto(
                p.Id,
                p.Name,
                p.PackageVersions.Count))
            .ToListAsync(cancellationToken);

        var response = new FilteredQueryResult<PackageListItemDto>(
            recordsTotal,
            recordsFiltered,
            data);

        return Success(response);
    }
}
