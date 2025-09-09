using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

public record GetAllIncubatorsQuery() : IBaseRequest<List<IncubatorDto>>;

public class GetAllIncubatorsQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetAllIncubatorsQuery, List<IncubatorDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<IncubatorDto>>> Handle(GetAllIncubatorsQuery request, CancellationToken cancellationToken)
    {
        var incubators = await repository.GetAllIncubators(cancellationToken);

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
