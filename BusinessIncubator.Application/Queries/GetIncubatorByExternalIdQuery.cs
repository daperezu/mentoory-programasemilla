using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

public sealed record GetIncubatorByExternalIdQuery(Guid ExternalId) : IBaseRequest<IncubatorByExternalIdDto>;

public sealed record IncubatorByExternalIdDto(
    long Id,
    Guid ExternalId,
    string Name,
    string Key,
    bool IsActive);

public sealed class GetIncubatorByExternalIdQueryHandler(
    IBusinessIncubatorRepository repository) : BaseCommandHandler<GetIncubatorByExternalIdQuery, IncubatorByExternalIdDto>
{
    public override async Task<Result<IncubatorByExternalIdDto>> Handle(GetIncubatorByExternalIdQuery request, CancellationToken cancellationToken)
    {
        var incubator = await repository.GetBusinessIncubatorByExternalIdAsync(request.ExternalId);
        if (incubator is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, ("ExternalId", "Incubadora no encontrada"));
        }

        var dto = new IncubatorByExternalIdDto(
            incubator.Id,
            incubator.ExternalId,
            incubator.Name,
            incubator.Key,
            !incubator.IsDeleted);

        return Success(dto);
    }
}