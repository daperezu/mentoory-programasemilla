using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Public.Commands;

/// <summary>
/// Handler for recording project interest.
/// </summary>
public class RecordProjectInterestCommandHandler : BaseCommandHandler<RecordProjectInterestCommand, RecordProjectInterestDto>
{
    private readonly IBusinessIncubatorRepository _repository;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<RecordProjectInterestCommandHandler> _logger;

    public RecordProjectInterestCommandHandler(
        IBusinessIncubatorRepository repository,
        ITimeProvider timeProvider,
        ILogger<RecordProjectInterestCommandHandler> logger)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public override async Task<Result<RecordProjectInterestDto>> Handle(
        RecordProjectInterestCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate interest type
            var validTypes = new[] { "View", "Contact", "Apply" };
            if (!validTypes.Contains(request.InterestType))
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(RecordProjectInterestCommand), $"Tipo de interés inválido: {request.InterestType}"));
            }

            // Get project by external ID
            var project = await _repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
            if (project is null)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(RecordProjectInterestCommand), "Proyecto no encontrado."));
            }

            // Calculate distance if both locations are available
            double? distanceKm = null;
            if (request.ObserverLatitude.HasValue &&
                request.ObserverLongitude.HasValue &&
                project.Latitude.HasValue &&
                project.Longitude.HasValue)
            {
                distanceKm = CalculateHaversineDistance(
                    request.ObserverLatitude.Value,
                    request.ObserverLongitude.Value,
                    project.Latitude.Value,
                    project.Longitude.Value);
            }

            // Check if interest already exists for authenticated users
            bool isNewInterest = true;
            if (!string.IsNullOrEmpty(request.UserId))
            {
                var existingInterest = await _repository.GetProjectInterestAsync(
                    project.Id,
                    request.UserId,
                    request.InterestType,
                    cancellationToken);

                if (existingInterest != null)
                {
                    isNewInterest = false;
                    _logger.LogInformation(
                        "User {UserId} already expressed {InterestType} interest in project {ProjectId}",
                        request.UserId,
                        request.InterestType,
                        project.Id);
                }
            }

            // Record the interest
            if (isNewInterest)
            {
                await _repository.RecordProjectInterestAsync(
                    project.Id,
                    request.UserId,
                    request.SessionId,
                    request.InterestType,
                    request.ObserverLatitude,
                    request.ObserverLongitude,
                    distanceKm,
                    request.UserAgent,
                    request.IpAddress,
                    request.ReferrerUrl,
                    _timeProvider.UtcNow,
                    cancellationToken);

                _logger.LogInformation(
                    "Recorded {InterestType} interest for project {ProjectId} from {Source}",
                    request.InterestType,
                    project.Id,
                    !string.IsNullOrEmpty(request.UserId) ? $"User {request.UserId}" : $"Session {request.SessionId}");
            }

            var result = new RecordProjectInterestDto
            {
                InterestId = 0, // We don't expose internal IDs
                ProjectId = project.ExternalId,
                InterestType = request.InterestType,
                RecordedAt = _timeProvider.UtcNow,
                DistanceKm = distanceKm.HasValue ? Math.Round(distanceKm.Value, 2) : null,
                IsNewInterest = isNewInterest,
                Message = isNewInterest
                    ? "Interés registrado exitosamente."
                    : "Ya has expresado interés en este proyecto."
            };

            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording project interest");
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(RecordProjectInterestCommand), "Error al registrar interés en el proyecto."));
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
}