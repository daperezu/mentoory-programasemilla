using LinaSys.Auth.Application.Interfaces;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Auth.Application.Queries.Context;

/// <summary>
/// Query to get user's accessible incubators based on role.
/// </summary>
public record GetUserActiveIncubatorsQuery(string UserId, string Role) : IBaseRequest<List<long>>;

/// <summary>
/// Handler for GetUserIncubatorsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetUserActiveIncubatorsQueryHandler"/> class.
/// </remarks>
/// <param name="incubatorAccessService">The incubator access service.</param>
public class GetUserActiveIncubatorsQueryHandler(IIncubatorAccessService incubatorAccessService) : BaseCommandHandler<GetUserActiveIncubatorsQuery, List<long>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<long>>> Handle(GetUserActiveIncubatorsQuery request, CancellationToken cancellationToken)
    {
        var incubators = await incubatorAccessService.GetUserActiveIncubatorsAsync(
            request.UserId,
            request.Role,
            cancellationToken).ConfigureAwait(false);

        return Success(incubators);
    }
}
