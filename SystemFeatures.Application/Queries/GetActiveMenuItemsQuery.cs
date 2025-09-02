using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.SystemFeatures.Domain.AggregatesModel.WebFeatureAggregate;
using MediatR;

namespace LinaSys.SystemFeatures.Application.Queries;

public record GetActiveMenuItemsQuery() : IBaseRequest<List<WebFeatureMenuDto>>;

public record WebFeatureMenuDto(
    long Id,
    string Name,
    string? Area,
    string? Controller,
    string? Action,
    long? ParentId,
    int MenuOrder);

public class GetActiveMenuItemsQueryHandler(
    IWebFeatureRepository repository) : BaseCommandHandler<GetActiveMenuItemsQuery, List<WebFeatureMenuDto>>
{
    public override async Task<Result<List<WebFeatureMenuDto>>> Handle(
        GetActiveMenuItemsQuery request,
        CancellationToken cancellationToken)
    {
        var menuItems = await repository.GetMenuHierarchyAsync(cancellationToken);

        var dtos = menuItems.Select(item => new WebFeatureMenuDto(
            item.Id,
            item.Name,
            item.Area,
            item.Controller,
            item.Action,
            item.ParentId,
            item.MenuOrder))
            .ToList();

        return Success(dtos);
    }
}