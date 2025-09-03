using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get an incubator by its ID.
/// </summary>
public record GetIncubatorByIdQuery(long IncubatorId) : IBaseRequest<IncubatorDto?>;

/// <summary>
/// DTO for incubator information.
/// </summary>
public class IncubatorDto
{
    /// <summary>
    /// Gets or sets the incubator identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the incubator is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Handler for GetIncubatorByIdQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetIncubatorByIdQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetIncubatorByIdQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetIncubatorByIdQuery, IncubatorDto?>
{

    /// <inheritdoc/>
    public override async Task<Result<IncubatorDto?>> Handle(
        GetIncubatorByIdQuery request,
        CancellationToken cancellationToken)
    {
        var incubator = await repository.GetByIdAsync(request.IncubatorId, cancellationToken);

        if (incubator is null)
        {
            return Success(null);
        }

        var dto = new IncubatorDto
        {
            Id = incubator.Id,
            Name = incubator.Name,
            Key = incubator.Key,
            IsDeleted = incubator.IsDeleted
        };

        return Success(dto);
    }
}