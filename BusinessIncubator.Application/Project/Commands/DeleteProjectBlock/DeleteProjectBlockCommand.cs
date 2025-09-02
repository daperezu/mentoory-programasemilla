using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectBlock;

/// <summary>
/// Command to delete a project block.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="BlockId">The block ID to delete.</param>
public sealed record DeleteProjectBlockCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long BlockId) : IBaseRequest;