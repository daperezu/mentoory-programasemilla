using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.SystemFeatures.Domain.AggregatesModel.WebFeatureAggregate;

namespace LinaSys.SystemFeatures.Application.Queries;

/// <summary>
/// Query to get a web feature by area, controller, and action.
/// </summary>
/// <param name="Area">The area of the web feature.</param>
/// <param name="Controller">The controller of the web feature.</param>
/// <param name="Action">The action of the web feature.</param>
public record GetWebFeatureByAreaControllerAndActionQuery(string Area, string Controller, string Action) : IBaseRequest<WebFeatureDto?>;

/// <summary>
/// Data transfer object for a web feature.
/// </summary>
/// <param name="ExternalId">The external identifier of the web feature.</param>
/// <param name="Name">The name of the web feature.</param>
/// <param name="IsPublic">Indicates whether the web feature is public.</param>
public record WebFeatureDto(Guid ExternalId, string Name, bool IsPublic);

/// <summary>
/// Handler for the GetWebFeatureByAreaControllerAndActionQuery.
/// </summary>
public class GetWebFeatureByAreaControllerAndActionQueryHandler(IWebFeatureRepository webFeatureRepository)
    : BaseCommandHandler<GetWebFeatureByAreaControllerAndActionQuery, WebFeatureDto?>
{
    /// <summary>
    /// Handles the GetWebFeatureByAreaControllerAndActionQuery.
    /// </summary>
    /// <param name="request">The query request containing area, controller, and action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the web feature DTO or null if not found.</returns>
    public override async Task<Result<WebFeatureDto?>> Handle(GetWebFeatureByAreaControllerAndActionQuery request, CancellationToken cancellationToken)
    {
        var feature = await webFeatureRepository.GetByAreaControllerAndActionAsync(request.Area, request.Controller, request.Action, cancellationToken);
        var result = feature is null ? null : new WebFeatureDto(feature.ExternalId, feature.Name, feature.IsPublic);
        return Success(result);
    }
}
