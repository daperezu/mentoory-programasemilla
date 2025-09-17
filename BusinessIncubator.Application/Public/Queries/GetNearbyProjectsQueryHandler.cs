using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Infrastructure.Geolocation;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Public.Queries;

public class GetNearbyProjectsQueryHandler : BaseCommandHandler<GetNearbyProjectsQuery, NearbyProjectsDto>
{
    private readonly IBusinessIncubatorRepository _repository;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<GetNearbyProjectsQueryHandler> _logger;

    public GetNearbyProjectsQueryHandler(
        IBusinessIncubatorRepository repository,
        ITimeProvider timeProvider,
        ILogger<GetNearbyProjectsQueryHandler> logger)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public override async Task<Result<NearbyProjectsDto>> Handle(
        GetNearbyProjectsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (request.Latitude < -90 || request.Latitude > 90)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(GetNearbyProjectsQuery), "La latitud debe estar entre -90 y 90 grados."));
            }

            if (request.Longitude < -180 || request.Longitude > 180)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(GetNearbyProjectsQuery), "La longitud debe estar entre -180 y 180 grados."));
            }

            if (request.RadiusKm <= 0 || request.RadiusKm > 100)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(GetNearbyProjectsQuery), "El radio de búsqueda debe estar entre 0 y 100 km."));
            }

            // Calculate bounding box for initial filtering
            var bounds = GetBoundingBox(
                (double)request.Latitude,
                (double)request.Longitude,
                request.RadiusKm);

            // Generate geohash prefixes to search
            var centerGeohash = GeohashHelper.Encode((double)request.Latitude, (double)request.Longitude, 5);
            var geohashPrefixes = new HashSet<string> { centerGeohash };

            // Add neighbor geohashes for better coverage at boundaries
            var neighbors = GeohashHelper.GetNeighbors(centerGeohash);
            foreach (var neighbor in neighbors)
            {
                geohashPrefixes.Add(neighbor);
            }

            _logger.LogDebug(
                "Searching in geohash prefixes: {Prefixes} for radius {Radius}km",
                string.Join(", ", geohashPrefixes),
                request.RadiusKm);

            // Query projects using geohash prefixes and bounding box
            var candidateProjects = await _repository.GetProjectsInGeohashesAsync(
                geohashPrefixes,
                (decimal)bounds.MinLat,
                (decimal)bounds.MaxLat,
                (decimal)bounds.MinLon,
                (decimal)bounds.MaxLon,
                cancellationToken);

            // Calculate precise distances and filter by radius
            var nearbyProjects = new List<NearbyProjectDto>();
            foreach (var project in candidateProjects)
            {
                if (!project.Latitude.HasValue || !project.Longitude.HasValue)
                {
                    continue;
                }

                // Calculate Haversine distance
                var distance = CalculateHaversineDistance(
                    request.Latitude,
                    request.Longitude,
                    project.Latitude.Value,
                    project.Longitude.Value);

                if (distance <= request.RadiusKm)
                {
                    nearbyProjects.Add(new NearbyProjectDto
                    {
                        ExternalId = project.ExternalId,
                        Name = project.Name,
                        Description = project.Description,
                        HeroImageBlobId = project.HeroImageBlobId,
                        HeroImageUrl = null, // Will be populated by the controller/view
                        Latitude = project.Latitude.Value,
                        Longitude = project.Longitude.Value,
                        LocationName = project.LocationName,
                        LocationAddress = project.LocationAddress,
                        DistanceKm = Math.Round(distance, 2),
                        BusinessIncubatorName = string.Empty, // TODO: Will be resolved with proper repository method
                        ActiveParticipants = 0, // TODO: Will be resolved with proper repository method
                        LastActivityDate = project.UpdatedAt,
                        RegistrationStartDate = null, // TODO: Get from ProjectStages
                        RegistrationEndDate = null, // TODO: Get from ProjectStages
                        CurrentPhase = "Activo" // TODO: Get from active ProjectStage
                    });
                }
            }

            // Sort by distance and limit results
            var sortedProjects = nearbyProjects
                .OrderBy(p => p.DistanceKm)
                .Take(request.MaxResults)
                .ToList();

            var result = new NearbyProjectsDto
            {
                UserLatitude = request.Latitude,
                UserLongitude = request.Longitude,
                SearchRadiusKm = request.RadiusKm,
                Projects = sortedProjects,
                TotalFound = sortedProjects.Count,
                SearchedAt = _timeProvider.UtcNow
            };

            _logger.LogInformation(
                "Found {Count} projects within {Radius}km of ({Lat}, {Lon})",
                result.TotalFound,
                request.RadiusKm,
                request.Latitude,
                request.Longitude);

            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for nearby projects");
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetNearbyProjectsQuery), "Error al buscar proyectos cercanos."));
        }
    }

    private static double CalculateHaversineDistance(
        decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double EarthRadiusKm = 6371.0;

        double lat1Rad = ToRadians((double)lat1);
        double lat2Rad = ToRadians((double)lat2);
        double deltaLat = ToRadians((double)(lat2 - lat1));
        double deltaLon = ToRadians((double)(lon2 - lon1));

        double a = (Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)) +
                   (Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2));

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    private static (double MinLat, double MaxLat, double MinLon, double MaxLon) GetBoundingBox(
        double latitude, double longitude, double radiusKm)
    {
        const double EarthRadiusKm = 6371.0;

        // Calculate latitude bounds
        double latDelta = (radiusKm / EarthRadiusKm) * (180 / Math.PI);
        double minLat = latitude - latDelta;
        double maxLat = latitude + latDelta;

        // Calculate longitude bounds (accounting for latitude)
        double lonDelta = (radiusKm / (EarthRadiusKm * Math.Cos(latitude * Math.PI / 180))) * (180 / Math.PI);
        double minLon = longitude - lonDelta;
        double maxLon = longitude + lonDelta;

        // Clamp values to valid ranges
        minLat = Math.Max(-90, minLat);
        maxLat = Math.Min(90, maxLat);
        minLon = Math.Max(-180, minLon);
        maxLon = Math.Min(180, maxLon);

        return (minLat, maxLat, minLon, maxLon);
    }
}