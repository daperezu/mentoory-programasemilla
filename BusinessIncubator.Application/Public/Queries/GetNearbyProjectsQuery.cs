using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Public.Queries;

public record GetNearbyProjectsQuery(
    decimal Latitude,
    decimal Longitude,
    double RadiusKm = 15.0,
    int MaxResults = 20) : IBaseRequest<NearbyProjectsDto>;