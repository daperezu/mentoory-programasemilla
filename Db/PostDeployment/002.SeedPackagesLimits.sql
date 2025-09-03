-- ==========================================================================================
-- Post-Deployment Script for Seeding default Packages and Limits for the Subscription Module
-- ==========================================================================================

; -- Sepparator semicolon before WITH statement

-- 1. MERGE: Packages
MERGE [subscription].[Packages] AS target
USING (SELECT N'Default' AS Name) AS source
ON target.Name = source.Name
WHEN MATCHED THEN 
    UPDATE SET UpdatedAt = GETUTCDATE(), UpdatedBy = 'SeedScript'
WHEN NOT MATCHED THEN 
    INSERT (Name, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
    VALUES (source.Name, 'SeedScript', '2025-01-01 00:00:00', NULL, NULL);

-- 2. Capture PackageId
DECLARE @PackageId BIGINT;
SELECT @PackageId = Id FROM [subscription].[Packages] WHERE Name = N'Default';

-- 3. MERGE: PackageVersions
MERGE [subscription].[PackageVersions] AS target
USING (SELECT @PackageId AS PackageId, N'Default Version' AS Label) AS source
ON target.PackageId = source.PackageId AND target.Label = source.Label
WHEN MATCHED THEN 
    UPDATE SET UpdatedAt = GETUTCDATE(), UpdatedBy = 'SeedScript'
WHEN NOT MATCHED THEN 
    INSERT (PackageId, Label, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
    VALUES (source.PackageId, source.Label, 'SeedScript', '2025-01-01 00:00:00', NULL, NULL);

-- 4. Capture PackageVersionId
DECLARE @PackageVersionId BIGINT;
SELECT @PackageVersionId = Id FROM [subscription].[PackageVersions]
WHERE PackageId = @PackageId AND Label = N'Default Version';

-- 5. MERGE: PackageVersionLimits
MERGE [subscription].[PackageVersionLimits] AS target
USING (
    SELECT @PackageVersionId AS PackageVersionId, 1 AS Type, 1 AS Quantity
    UNION ALL
    SELECT @PackageVersionId, 2, 1
) AS source
ON target.PackageVersionId = source.PackageVersionId AND target.Type = source.Type
WHEN MATCHED THEN 
    UPDATE SET Quantity = source.Quantity
WHEN NOT MATCHED THEN 
    INSERT (PackageVersionId, Type, Quantity)
    VALUES (source.PackageVersionId, source.Type, source.Quantity);
