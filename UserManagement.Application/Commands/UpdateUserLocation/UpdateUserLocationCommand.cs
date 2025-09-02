using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.UserManagement.Application.Commands.UpdateUserLocation;

public record UpdateUserLocationCommand(
    string UserId,
    string? Country,
    string? Province,
    string? Canton,
    string? District,
    string? FullAddress) : IBaseRequest;