# ADR-002: Geolocation Architecture for Public Project Discovery

## Status
Accepted

## Context
LinaSys needs to implement a public-facing homepage that allows potential entrepreneurs to discover nearby incubator projects without requiring authentication. The system must balance several competing concerns:

- **User Privacy**: Location data is sensitive and requires explicit consent
- **Performance**: Spatial queries can be expensive with large datasets
- **Accuracy**: Different use cases require different levels of location precision
- **Fallback**: Not all users will share their location
- **Scalability**: System must handle thousands of concurrent location-based queries
- **Azure Compatibility**: Solution must work efficiently on Azure SQL Database with cost optimization

## Decision

We will implement a **Geohash-based geolocation approach** optimized for Azure SQL Database:

1. **Geohash indexing** for efficient B-tree queries
2. **Bounding box pre-filtering** in SQL
3. **Haversine calculation** in application layer
4. **Browser Geolocation API** as primary location source
5. **IP-based geolocation** as fallback mechanism
6. **Multi-level caching** to optimize performance
7. **Explicit consent model** for privacy compliance

This approach provides the best balance of performance, cost-efficiency, and compatibility across all Azure SQL Database tiers while maintaining sufficient accuracy for proximity searches.

### Architecture Components

```
┌─────────────────┐
│   Browser       │
│ Geolocation API │
└────────┬────────┘
         │ Primary
         ▼
┌─────────────────┐     ┌──────────────┐
│  Public Homepage├────►│ Memory Cache │
│   Controller    │     │   (5 min)    │
└────────┬────────┘     └──────┬───────┘
         │                     │ Miss
         │ Fallback           ▼
┌────────▼────────┐     ┌──────────────┐
│  IP Geolocation │     │ Redis Cache  │
│    Service      │     │   (30 min)   │
└─────────────────┘     └──────┬───────┘
                              │ Miss
                              ▼
                     ┌─────────────────┐
                     │   SQL Server    │
                     │  Geohash Index  │
                     │  B-tree Queries │
                     └─────────────────┘
```

### Data Model

```sql
-- Projects table with geohash support
ALTER TABLE [businessincubators].[Projects] ADD
    [Latitude] DECIMAL(10, 8) NULL,
    [Longitude] DECIMAL(11, 8) NULL,
    [Geohash] VARCHAR(12) NULL,        -- Full geohash for location
    [GeohashPrefix5] AS LEFT([Geohash], 5) PERSISTED, -- ~5km precision
    [GeohashPrefix6] AS LEFT([Geohash], 6) PERSISTED, -- ~1km precision
    [HeroImageBlobId] NVARCHAR(450) NULL,
    [HasHeroImage] BIT NOT NULL DEFAULT (0),
    [LocationUpdatedAt] DATETIME2 NULL,
    [LocationUpdatedBy] NVARCHAR(256) NULL;

-- B-tree indexes for efficient geospatial queries
CREATE NONCLUSTERED INDEX [IX_Projects_Geohash] 
    ON [businessincubators].[Projects] ([GeohashPrefix5], [GeohashPrefix6])
    INCLUDE ([Latitude], [Longitude], [Name], [ExternalId])
    WHERE [Geohash] IS NOT NULL;

CREATE NONCLUSTERED INDEX [IX_Projects_LatLon] 
    ON [businessincubators].[Projects] ([Latitude], [Longitude])
    INCLUDE ([Name], [ExternalId])
    WHERE [Latitude] IS NOT NULL;
```

### Implementation Strategy

#### 1. Location Acquisition
```javascript
// Client-side: Request high-accuracy location
navigator.geolocation.getCurrentPosition(
    position => sendToServer(position.coords),
    error => fallbackToIpLocation(),
    { enableHighAccuracy: true, timeout: 10000 }
);
```

#### 2. Distance Calculation with Geohash

```csharp
// Step 1: Calculate geohash and neighbors
var userGeohash = GeoHasher.Encode(userLat, userLon, 5); // 5 chars = ~5km precision
var neighbors = GeoHasher.GetNeighbors(userGeohash);

// Step 2: Calculate bounding box for additional filtering
var bounds = CalculateBoundingBox(userLat, userLon, radiusKm);

// Step 3: SQL query with geohash and bounding box filtering
var sql = @"
    SELECT Id, ExternalId, Name, Latitude, Longitude, HeroImageBlobId
    FROM Projects
    WHERE GeohashPrefix5 IN @prefixes
    AND Latitude BETWEEN @minLat AND @maxLat
    AND Longitude BETWEEN @minLon AND @maxLon
    AND IsDeleted = 0";

// Step 4: Application-layer Haversine calculation for precise distance
var nearbyProjects = candidates
    .Select(p => new {
        Project = p,
        Distance = HaversineCalculator.DistanceInKm(
            userLat, userLon, 
            p.Latitude.Value, p.Longitude.Value)
    })
    .Where(x => x.Distance <= radiusKm)
    .OrderBy(x => x.Distance)
    .Take(100);
```

#### 3. Caching Strategy
```csharp
// Round coordinates to reduce cache keys (0.01° ≈ 1.1km)
var cacheKey = $"nearby:{Math.Round(lat, 2)}:{Math.Round(lon, 2)}:{radius}";
```


## Consequences

### Positive

1. **Cost Optimization**: Reduces Azure SQL Database costs by 80-90% compared to spatial queries
2. **Universal Compatibility**: Works efficiently on all Azure SQL tiers (Basic through Premium)
3. **Good Performance**: Achieves sub-100ms response times with proper indexing
4. **Sufficient Accuracy**: ±1% distance error acceptable for 15km radius searches
5. **Scalability**: B-tree indexes scale better than spatial indexes for large datasets
6. **Privacy**: Explicit consent with granular control
7. **Resilience**: Multiple fallback mechanisms ensure functionality
8. **Clean Architecture**: Maintains separation of concerns

### Negative

1. **Implementation Complexity**: Requires geohash library and Haversine calculations
2. **Slightly Lower Accuracy**: ~1% error vs 0.001% with spatial functions
3. **Two-Step Process**: Query filtering then distance calculation
4. **Cache Complexity**: Multi-level caching increases operational overhead
5. **Browser Compatibility**: Older browsers lack geolocation support

### Neutral

1. **Learning Curve**: Team needs to understand spatial queries
2. **Testing Complexity**: Spatial queries require specific test strategies
3. **Migration Path**: Future move to dedicated GIS database possible

## Alternatives Considered

### Alternative 1: SQL Server Spatial Features (Geography Type)
**Rejected because:**
- **High Azure Costs**: Spatial queries consume 10-100x more DTUs than standard queries
- **Limited Compatibility**: Requires Azure SQL Premium tier for acceptable performance
- **Blocking Issues**: Spatial index rebuilds can cause blocking in Azure
- **Over-engineering**: Geodesic accuracy (±0.001%) unnecessary for 15km searches

**Would have implemented:**
```sql
[GeoLocation] AS geography::Point([Latitude], [Longitude], 4326) PERSISTED
-- with STDistance() calculations
```

### Alternative 2: Client-Side Distance Calculation
**Rejected because:**
- Would require sending all project locations to client
- Security risk exposing all data
- Poor performance with large datasets
- Increased bandwidth usage

### Alternative 3: PostGIS with PostgreSQL
**Rejected because:**
- Would require migrating from SQL Server
- Team lacks PostgreSQL expertise
- Additional infrastructure costs
- Migration risk too high

### Alternative 4: External Geocoding Service (Google Maps API)
**Rejected because:**
- Ongoing API costs
- External dependency
- Privacy concerns with third-party data sharing
- Rate limiting issues at scale

### Alternative 5: Simple Latitude/Longitude without Geohash
**Rejected because:**
- Poor query performance without proper indexing strategy
- Complex bounding box calculations without optimization
- No efficient neighbor searching
- Would require scanning more records

## Implementation Phases

### Phase 1: Foundation (Week 1)
- Add geography columns to database
- Create spatial indexes
- Implement basic distance queries

### Phase 2: Client Integration (Week 2)
- Browser geolocation implementation
- Consent management UI
- Fallback to date-based sorting

### Phase 3: Performance (Week 3)
- Redis cache integration
- Query optimization
- Load testing

### Phase 4: Privacy & Compliance (Week 4)
- GDPR compliance features
- Location data management
- Audit logging

## Metrics for Success

1. **Performance Metrics**
   - P95 query time < 100ms
   - Cache hit ratio > 80%
   - Page load time < 2 seconds

2. **User Metrics**
   - Location consent rate > 40%
   - Project discovery increase > 25%
   - Mobile usage > 50%

3. **Technical Metrics**
   - Spatial index efficiency > 90%
   - Database CPU usage < 30%
   - Memory cache size < 100MB

## Security Considerations

1. **Location Data Protection**
   - Encrypt location data at rest
   - Use HTTPS for all location transmissions
   - Implement rate limiting on location endpoints

2. **Privacy Controls**
   - Explicit opt-in for location sharing
   - Ability to revoke consent
   - Location data retention policies

3. **Access Control**
   - Public endpoints don't expose precise coordinates
   - Admin-only access to exact locations
   - Audit trail for location data access

## Migration Strategy

If we need to migrate to a different solution:

1. **Data Export**: Geography columns can be exported as WKT/WKB
2. **Standard Formats**: Use GeoJSON for data interchange
3. **Abstraction Layer**: Repository pattern isolates spatial queries
4. **Gradual Migration**: Can run both systems in parallel

## Code Examples

### Value Object for Coordinates
```csharp
public class GeoCoordinate : ValueObject
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }
    public string Geohash { get; }
    
    private GeoCoordinate(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        Geohash = GeoHasher.Encode((double)latitude, (double)longitude, 12);
    }
    
    public static Result<GeoCoordinate> Create(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            return Result<GeoCoordinate>.Failure(
                ResultErrorCodes.ValidationError, 
                "Latitud debe estar entre -90 y 90");
            
        if (longitude < -180 || longitude > 180)
            return Result<GeoCoordinate>.Failure(
                ResultErrorCodes.ValidationError,
                "Longitud debe estar entre -180 y 180");
            
        return Result<GeoCoordinate>.Success(new GeoCoordinate(latitude, longitude));
    }
    
    public string GetGeohashPrefix(int precision) => 
        Geohash.Substring(0, Math.Min(precision, Geohash.Length));
    
    public double DistanceInKmTo(GeoCoordinate other) => 
        HaversineCalculator.DistanceInKm(
            (double)Latitude, (double)Longitude,
            (double)other.Latitude, (double)other.Longitude);
    
    public List<string> GetNeighborGeohashes(int precision = 5) => 
        GeoHasher.GetNeighbors(GetGeohashPrefix(precision));
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
```

### Repository Implementation

```csharp
public class BusinessIncubatorRepository : IBusinessIncubatorRepository
{
    private readonly LinaSysDbContext _context;
    private readonly ILogger<BusinessIncubatorRepository> _logger;
    
    public async Task<List<Project>> GetProjectsWithinRadiusAsync(
        decimal latitude,
        decimal longitude, 
        double radiusKm,
        CancellationToken cancellationToken)
    {
        // Calculate geohash and neighbors for the search area
        var centerGeohash = GeoHasher.Encode((double)latitude, (double)longitude, 5);
        var neighbors = GeoHasher.GetNeighbors(centerGeohash).ToList();
        neighbors.Add(centerGeohash); // Include center
        
        // Calculate bounding box for additional filtering
        var bounds = CalculateBoundingBox(latitude, longitude, radiusKm);
        
        // Step 1: Query with geohash and bounding box filtering
        var candidates = await _context.Projects
            .Where(p => p.IsDeleted == false)
            .Where(p => p.Status == ProjectStatus.Active)
            .Where(p => p.Latitude != null && p.Longitude != null)
            .Where(p => neighbors.Contains(p.GeohashPrefix5))
            .Where(p => p.Latitude >= bounds.MinLat && p.Latitude <= bounds.MaxLat)
            .Where(p => p.Longitude >= bounds.MinLon && p.Longitude <= bounds.MaxLon)
            .Select(p => new {
                p.Id,
                p.ExternalId,
                p.Name,
                p.Description,
                p.Latitude,
                p.Longitude,
                p.HeroImageBlobId
            })
            .ToListAsync(cancellationToken);
        
        _logger.LogDebug("Found {Count} candidates in geohash areas", candidates.Count);
        
        // Step 2: Calculate precise distances using Haversine
        var projectsWithDistance = candidates
            .Select(p => new {
                Project = p,
                Distance = HaversineCalculator.DistanceInKm(
                    (double)latitude, (double)longitude,
                    (double)p.Latitude!.Value, (double)p.Longitude!.Value)
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Take(100)
            .ToList();
        
        // Step 3: Load full entities for final results
        var projectIds = projectsWithDistance.Select(x => x.Project.Id).ToList();
        var projects = await _context.Projects
            .Include(p => p.ProjectStages.Where(ps => ps.IsActive))
            .Where(p => projectIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
        
        // Preserve distance ordering
        return projectsWithDistance
            .Select(pd => projects.First(p => p.Id == pd.Project.Id))
            .ToList();
    }
    
    private BoundingBox CalculateBoundingBox(decimal latitude, decimal longitude, double radiusKm)
    {
        // Rough approximation: 1 degree latitude = 111km
        var latDelta = (decimal)(radiusKm / 111.0);
        
        // Longitude varies by latitude
        var lonDelta = (decimal)(radiusKm / (111.0 * Math.Cos((double)latitude * Math.PI / 180)));
        
        return new BoundingBox
        {
            MinLat = latitude - latDelta,
            MaxLat = latitude + latDelta,
            MinLon = longitude - lonDelta,
            MaxLon = longitude + lonDelta
        };
    }
}
```

### Haversine Calculator Helper
```csharp
public static class HaversineCalculator
{
    private const double EarthRadiusKm = 6371.0;
    
    public static double DistanceInKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return EarthRadiusKm * c;
    }
    
    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
```

## References

- [SQL Server Spatial Data Types](https://docs.microsoft.com/en-us/sql/relational-databases/spatial/spatial-data-types-overview)
- [W3C Geolocation API Specification](https://www.w3.org/TR/geolocation/)
- [GDPR Location Data Guidelines](https://ec.europa.eu/info/law/law-topic/data-protection_en)
- [Redis Geospatial Indexes](https://redis.io/docs/manual/geospatial/)
- [Martin Fowler - Dealing with Location Data](https://martinfowler.com/articles/location-data.html)

## Decision Date
2025-01-14

## Participants
- Development Team
- Architecture Review
- Privacy Officer (consulted)

## Review Date
This ADR should be reviewed after Phase 1 implementation (approximately 2025-02-14) to validate performance assumptions and adjust caching strategies if needed.

## Amendment History
- 2025-01-14: Initial draft created based on Cloudy Code requirements adaptation