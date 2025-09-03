using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Permissions.Application.ProtectedResource.Queries;

public record GetProtectedResourceByExternalIdQuery(Guid ExternalId) : IBaseRequest<ProtectedResourceDto>;

public record ProtectedResourceDto(int ResourceType, Guid ExternalId, long InternalId, string Name);

public class GetProtectedResourceByExternalIdQueryHandler(IProtectedResourceRepository protectedResourceRepository)
    : BaseCommandHandler<GetProtectedResourceByExternalIdQuery, ProtectedResourceDto>
{
    public override async Task<Result<ProtectedResourceDto>> Handle(GetProtectedResourceByExternalIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await protectedResourceRepository.GetProtectedResourceByExternalIdAsync(request.ExternalId, cancellationToken);

        return entity is null
            ? Failure(ResultErrorCodes.Auth_ProtectedResourceNotFound)
            : Success(new ProtectedResourceDto(entity.ResourceType, entity.ExternalId, entity.Id, entity.Name));
    }
}
