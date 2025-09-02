using LinaSys.Auth.Application.Queries.Context;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using MediatR;

namespace LinaSys.Orchestration.Application.Context;

/// <summary>
/// Orchestration query to get user's incubators with enriched data from BusinessIncubator domain.
/// </summary>
public record GetEnrichedUserActiveIncubatorsQuery(string UserId, string Role) : IBaseRequest<List<EnrichedIncubatorDto>>;

/// <summary>
/// DTO for enriched incubator information.
/// </summary>
public class EnrichedIncubatorDto
{
    /// <summary>
    /// Gets or sets the incubator identifier.
    /// </summary>
    public long IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator key.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Handler for GetEnrichedUserIncubatorsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetEnrichedUserActiveIncubatorsQueryHandler"/> class.
/// </remarks>
/// <param name="mediator">The mediator.</param>
public class GetEnrichedUserActiveIncubatorsQueryHandler(IMediator mediator) : BaseCommandHandler<GetEnrichedUserActiveIncubatorsQuery, List<EnrichedIncubatorDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<EnrichedIncubatorDto>>> Handle(
        GetEnrichedUserActiveIncubatorsQuery request,
        CancellationToken cancellationToken)
    {
        var authQuery = new GetUserActiveIncubatorsQuery(request.UserId, request.Role);
        var authResult = await mediator.Send(authQuery, cancellationToken);

        if (!authResult.IsSuccess)
        {
            return Failure(ResultErrorCodes.GenericError, ("GetEnrichedUserIncubators", "Error al obtener acceso a incubadoras"));
        }

        var incubatorIds = authResult.Value ?? [];

        if (!incubatorIds.Any())
        {
            if (request.Role == Roles.GlobalAdministrator)
            {
                var allIncubatorsQuery = new GetAllIncubatorsQuery();
                var allIncubatorsResult = await mediator.Send(allIncubatorsQuery, cancellationToken);
                if (!allIncubatorsResult.IsSuccess)
                {
                    return Failure(ResultErrorCodes.GenericError, ("GetEnrichedUserIncubators", "Error al obtener información de incubadoras"));
                }

                var allEnrichedIncubators = allIncubatorsResult.Value?.Select(inc => new EnrichedIncubatorDto
                {
                    IncubatorId = inc.Id,
                    Name = inc.Name,
                    Key = inc.Key
                }).ToList() ?? [];

                return Success(allEnrichedIncubators);
            }

            return Success([]);
        }

        var incubatorsQuery = new GetIncubatorsByIdsQuery(incubatorIds);
        var incubatorsResult = await mediator.Send(incubatorsQuery, cancellationToken);

        if (!incubatorsResult.IsSuccess)
        {
            return Failure(ResultErrorCodes.GenericError, ("GetEnrichedUserIncubators", "Error al obtener información de incubadoras"));
        }

        var enrichedIncubators = incubatorsResult.Value?.Select(inc => new EnrichedIncubatorDto
        {
            IncubatorId = inc.Id,
            Name = inc.Name,
            Key = inc.Key
        }).ToList() ?? [];

        return Success(enrichedIncubators);
    }
}
