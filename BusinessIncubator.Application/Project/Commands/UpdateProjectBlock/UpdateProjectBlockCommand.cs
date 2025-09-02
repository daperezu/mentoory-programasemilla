using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectBlock;

/// <summary>
/// Command to update a project block.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="BlockId">The block ID to update.</param>
/// <param name="Name">The new name for the block.</param>
/// <param name="Order">The new order for the block.</param>
public sealed record UpdateProjectBlockCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long BlockId,
    string Name,
    int Order) : IBaseRequest;