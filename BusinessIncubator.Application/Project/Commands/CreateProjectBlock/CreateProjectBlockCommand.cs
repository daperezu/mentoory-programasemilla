using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.CreateProjectBlock;

/// <summary>
/// Command to create a new project block.
/// </summary>
public sealed record CreateProjectBlockCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    string Name,
    int Order) : IBaseRequest<long>;