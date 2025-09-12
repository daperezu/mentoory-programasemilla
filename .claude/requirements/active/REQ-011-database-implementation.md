# REQ-011 Database Implementation Guide

## Overview
This document provides the database implementation approach for REQ-011 (Public Homepage with Geolocation) using SQL Server Database Project (SSDT) structure.

## Important Context
- **LinaSys is NOT in production** - Direct schema changes are allowed
- **SQL Database Project (SSDT)** is used for database management
- **No migrations needed** - Modify table definitions directly
- **Database folder**: `Db/` (not LindaDb)

## SSDT Structure Requirements

### One File Per Object Rule
SQL Server Database Projects require exactly one DDL statement per file:
- Each table in its own `.sql` file
- Each index in its own `.sql` file  
- Each stored procedure in its own `.sql` file
- Each function in its own `.sql` file

### Folder Organization
```
Db/
├── businessincubators/
│   ├── Tables/
│   │   ├── Projects.sql (MODIFY)
│   │   └── ProjectInterests.sql (CREATE)
│   ├── Indexes/
│   │   ├── IX_Projects_GeoLocation.sql (CREATE)
│   │   ├── IX_ProjectInterests_ProjectId.sql (CREATE)
│   │   └── IX_ProjectInterests_UserId.sql (CREATE)
│   ├── StoredProcedures/
│   │   ├── GetNearbyProjects.sql (CREATE)
│   │   └── GetUpcomingProjects.sql (CREATE)
│   └── Functions/
│       └── CalculateDistance.sql (CREATE)
├── usermanagement/
│   └── Tables/
│       └── UserProfiles.sql (MODIFY)
├── auth/
│   └── Tables/
│       └── AspNetRoles.sql (seed data only)
└── PostDeployment/
    ├── 011.SeedObserverRole.sql (CREATE)
    └── 012.SeedProjectLocations.sql (CREATE)
```

## Implementation Strategy: Geohash-Based Geolocation

LinaSys uses a **Geohash-based approach** for geolocation features, optimized for Azure SQL Database efficiency and cost-effectiveness.

### Why Geohash Over Spatial Features?
- **Cost Efficiency**: 80-90% reduction in Azure SQL DTU consumption
- **Universal Compatibility**: Works on all Azure SQL tiers
- **Sufficient Accuracy**: ±1% error acceptable for proximity searches
- **Better Scalability**: B-tree indexes outperform spatial indexes

## Implementation Steps

### Step 1: Modify Existing Tables

#### Projects Table
File: `Db/businessincubators/Tables/Projects.sql`

Add these columns after the `[Status]` column:
```sql
-- Geolocation fields for REQ-011 with geohash support
[Latitude] DECIMAL(10, 8) NULL,
[Longitude] DECIMAL(11, 8) NULL,
[Geohash] VARCHAR(12) NULL, -- Full geohash for location
[GeohashPrefix5] AS LEFT([Geohash], 5) PERSISTED, -- ~5km precision for indexing
[GeohashPrefix6] AS LEFT([Geohash], 6) PERSISTED, -- ~1km precision for fine-tuning
[HeroImageBlobId] NVARCHAR(450) NULL,
[HasHeroImage] BIT NOT NULL CONSTRAINT [DF_Projects_HasHeroImage] DEFAULT (0),
[LocationUpdatedAt] DATETIME2 NULL,
[LocationUpdatedBy] NVARCHAR(256) NULL,
```

#### UserProfiles Table  
File: `Db/usermanagement/Tables/UserProfiles.sql`

Add these columns:
```sql
-- Location consent fields for REQ-011
[LocationConsentAt] DATETIME2 NULL,
[LocationConsentRevoked] BIT NOT NULL CONSTRAINT [DF_UserProfiles_LocationConsentRevoked] DEFAULT (0),
[LocationConsentMethod] NVARCHAR(50) NULL,
[HomeLatitude] DECIMAL(10, 8) NULL,
[HomeLongitude] DECIMAL(11, 8) NULL,
[HomeLocationUpdatedAt] DATETIME2 NULL,
[PreferredSearchRadiusKm] INT NULL CONSTRAINT [DF_UserProfiles_PreferredRadius] DEFAULT (15),
```

### Step 2: Create New Tables

#### ProjectInterests Table
File: `Db/businessincubators/Tables/ProjectInterests.sql`

Complete table definition with all constraints and foreign keys.

### Step 3: Create Indexes

Each index must be in its own file in the appropriate `Indexes/` folder.

#### Geohash Index
File: `Db/businessincubators/Indexes/IX_Projects_Geohash.sql`
```sql
CREATE NONCLUSTERED INDEX [IX_Projects_Geohash] 
ON [businessincubators].[Projects] ([GeohashPrefix5], [GeohashPrefix6])
INCLUDE ([Latitude], [Longitude], [Name], [ExternalId])
WHERE [Geohash] IS NOT NULL
```

#### Latitude/Longitude Index
File: `Db/businessincubators/Indexes/IX_Projects_LatLon.sql`
```sql
CREATE NONCLUSTERED INDEX [IX_Projects_LatLon] 
ON [businessincubators].[Projects] ([Latitude], [Longitude])
INCLUDE ([Name], [ExternalId], [GeohashPrefix5])
WHERE [Latitude] IS NOT NULL AND [Longitude] IS NOT NULL
```

### Step 4: Create Stored Procedures

#### GetNearbyProjects
File: `Db/businessincubators/StoredProcedures/GetNearbyProjects.sql`

Returns candidates within bounding box for application-layer Haversine filtering:
```sql
-- Calculate bounding box
DECLARE @LatDelta DECIMAL(10, 8) = @RadiusKm / 111.0;
DECLARE @LonDelta DECIMAL(11, 8) = @RadiusKm / (111.0 * COS(RADIANS(@UserLatitude)));

-- Query using bounding box (actual distance calculation in app layer)
SELECT p.* FROM Projects p
WHERE p.Latitude BETWEEN (@UserLatitude - @LatDelta) AND (@UserLatitude + @LatDelta)
  AND p.Longitude BETWEEN (@UserLongitude - @LonDelta) AND (@UserLongitude + @LonDelta)
```

### Step 5: Post-Deployment Scripts

These scripts run after the schema is deployed:

#### Add Observer Role
File: `Db/PostDeployment/011.SeedObserverRole.sql`

Uses IF NOT EXISTS pattern for idempotency.

#### Seed Test Locations
File: `Db/PostDeployment/012.SeedProjectLocations.sql`

Adds Costa Rica coordinates to existing projects for testing.

### Step 6: Update Script.PostDeployment.sql

Add references to new post-deployment scripts:
```sql
PRINT '[011.SeedObserverRole.sql] Starting';
:r .\011.SeedObserverRole.sql
PRINT '[011.SeedObserverRole.sql] Finished';

PRINT '[012.SeedProjectLocations.sql] Starting';
:r .\012.SeedProjectLocations.sql
PRINT '[012.SeedProjectLocations.sql] Finished';
```

## Build and Deploy Process

### Build Database Project
```bash
cd Db
MSBuild LinaDb.sqlproj -p:Configuration=Debug
```

### Deploy to Local Database
The build creates a DACPAC file that can be deployed using:
- Visual Studio's Publish feature
- SqlPackage.exe command line tool
- Azure Data Studio

### Verify Changes
After deployment, verify:
1. New columns exist in Projects and UserProfiles tables
2. ProjectInterests table created
3. Spatial index created on Projects
4. Stored procedures accessible
5. Observer role exists
6. Sample location data populated

## Key Differences from Migration Approach

| Migration Approach | SSDT Approach |
|-------------------|---------------|
| ALTER TABLE statements | Modify table definition directly |
| Migration scripts with timestamps | Table definitions in source control |
| Rollback scripts | Source control history |
| Sequential execution | Declarative state |
| Manual deployment | DACPAC deployment |

## Advantages of SSDT for LinaSys

1. **Source Control**: Complete database schema in Git
2. **Compile-Time Validation**: Build catches schema errors
3. **Refactoring Support**: Rename operations handled automatically
4. **Schema Comparison**: Built-in diff tools
5. **Deployment Flexibility**: Multiple target environments

## Notes for Developers

- **Never use ALTER TABLE** in SSDT projects - modify the CREATE TABLE statement
- **Indexes need filtered conditions** when columns are nullable (e.g., WHERE [Latitude] IS NOT NULL)
- **Post-deployment scripts** must be idempotent (can run multiple times safely)
- **Geography columns** can be computed from Latitude/Longitude
- **Build frequently** to catch errors early

## Application Layer Implementation

### Geohash Calculation
The application must calculate geohash when saving location:
```csharp
project.SetLocation(latitude, longitude, auditContext);
// This internally calculates: Geohash = GeoHasher.Encode(lat, lon, 12)
```

### Distance Calculation
Use Haversine formula after database query:
```csharp
var distance = HaversineCalculator.DistanceInKm(
    userLat, userLon, 
    project.Latitude, project.Longitude);
```

## Troubleshooting

### Common Issues

1. **Build Error: "Only one statement allowed per batch"**
   - Solution: Split into separate files

2. **Geohash not populating**
   - Solution: Ensure application calculates geohash before save

3. **Index not being used**
   - Solution: Check query uses GeohashPrefix5 column

4. **Foreign key conflicts**
   - Solution: Check schema order in project file

5. **Post-deployment script fails**
   - Solution: Add IF EXISTS checks

## Related Documentation

- [REQ-011-public-homepage-geolocation.md](../pending/REQ-011-public-homepage-geolocation.md)
- [geolocation-technical-analysis.md](./geolocation-technical-analysis.md)
- [ADR-002-geolocation-architecture.md](../../architecture-decisions/ADR-002-geolocation-architecture.md)