using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get multiple incubators by their IDs.
/// </summary>
public record GetIncubatorsByIdsQuery(List<long> IncubatorIds) : IBaseRequest<List<IncubatorDto>>;

/// <summary>
/// Handler for GetIncubatorsByIdsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetIncubatorsByIdsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetIncubatorsByIdsQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetIncubatorsByIdsQuery, List<IncubatorDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<IncubatorDto>>> Handle(
        GetIncubatorsByIdsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all incubators in a single optimized query
        var incubators = await repository.GetByIdsAsync(request.IncubatorIds, cancellationToken);

        // Map to DTOs
        var incubatorDtos = incubators.Select(incubator => new IncubatorDto
        {
            Id = incubator.Id,
            Name = incubator.Name,
            Key = incubator.Key,
            IsDeleted = incubator.IsDeleted
        }).ToList();

        return Success(incubatorDtos);
    }
}
