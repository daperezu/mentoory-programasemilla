using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Public.Queries;

/// <summary>
/// Query to get detailed information about a project for public viewing.
/// </summary>
public record GetProjectDetailsQuery(Guid ExternalId) : IBaseRequest<ProjectDetailDto>;