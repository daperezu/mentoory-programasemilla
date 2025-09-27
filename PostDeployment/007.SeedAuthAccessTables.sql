-- =============================================
-- Seed Data for Auth Domain Access Tables
-- Created: 2025-01-13
-- Description: Mirrors existing relationships from BusinessIncubator domain into Auth read models
-- This ensures Auth services have access control data on fresh install
-- IMPORTANT: This script only affects demo data and is safe to run multiple times
-- =============================================

PRINT '[007.SeedAuthAccessTables.sql] Starting';

-- Get demo user IDs for safe cleanup
DECLARE @DemoStarterId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.starter');
DECLARE @DemoMentorId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.mentor');
DECLARE @DemoCoordinatorId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.coordinator');
DECLARE @DemoAdminId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.admin');

-- Get demo incubator and project IDs
DECLARE @DemoIncubatorId BIGINT = (SELECT TOP 1 Id FROM [businessincubators].[BusinessIncubators] WHERE [Key] = 'DEMO');
DECLARE @DemoProjectId BIGINT = (SELECT TOP 1 Id FROM [businessincubators].[Projects] WHERE [Key] = 'INNOV-DEMO');

-- =============================================
-- UserProjectAccess: Mirror businessincubators.ProjectUsers
-- =============================================
PRINT '[007.SeedAuthAccessTables.sql] Seeding UserProjectAccess from ProjectUsers';

-- Only clear demo-specific data to avoid destroying real user access
DELETE FROM [dbo].[UserProjectAccess]
WHERE UserId IN (@DemoStarterId, @DemoMentorId, @DemoCoordinatorId, @DemoAdminId)
   OR ProjectId = @DemoProjectId
   OR IncubatorId = @DemoIncubatorId;

-- Use MERGE to safely sync records from businessincubators.ProjectUsers
-- Only processes demo-related records
MERGE [dbo].[UserProjectAccess] AS target
USING (
    SELECT 
        pu.UserId,
        pu.ProjectId,
        p.BusinessIncubatorId AS IncubatorId,
        pu.Role,
        pu.IsActive,
        '2024-01-01 00:00:00' AS LastSyncedAt
    FROM [businessincubators].[ProjectUsers] pu
    INNER JOIN [businessincubators].[Projects] p ON pu.ProjectId = p.Id
    INNER JOIN [dbo].[AspNetUsers] u ON pu.UserId = u.Id
    WHERE pu.IsActive = 1
      -- Only sync demo-related records
      AND (pu.UserId IN (@DemoStarterId, @DemoMentorId, @DemoCoordinatorId, @DemoAdminId)
           OR pu.ProjectId = @DemoProjectId
           OR p.BusinessIncubatorId = @DemoIncubatorId)
) AS source ON target.UserId = source.UserId AND target.ProjectId = source.ProjectId
WHEN MATCHED THEN
    UPDATE SET 
        IncubatorId = source.IncubatorId,
        Role = source.Role,
        IsActive = source.IsActive,
        LastSyncedAt = source.LastSyncedAt
WHEN NOT MATCHED THEN
    INSERT ([UserId], [ProjectId], [IncubatorId], [Role], [IsActive], [LastSyncedAt])
    VALUES (source.UserId, source.ProjectId, source.IncubatorId, source.Role, source.IsActive, source.LastSyncedAt);

DECLARE @ProjectAccessCount INT = @@ROWCOUNT;
PRINT '[007.SeedAuthAccessTables.sql] Inserted ' + CAST(@ProjectAccessCount AS NVARCHAR(10)) + ' UserProjectAccess records';

-- =============================================
-- UserIncubatorAccess: Create incubator-level access
-- Note: All users with project access need incubator access too
-- =============================================
PRINT '[007.SeedAuthAccessTables.sql] Seeding UserIncubatorAccess';

-- Only clear demo-specific incubator access
DELETE FROM [dbo].[UserIncubatorAccess]
WHERE UserId IN (@DemoStarterId, @DemoMentorId, @DemoCoordinatorId, @DemoAdminId)
   OR IncubatorId = @DemoIncubatorId;

-- Use MERGE for safe incubator access sync (all demo users with project access)
MERGE [dbo].[UserIncubatorAccess] AS target
USING (
    SELECT DISTINCT
        pu.UserId,
        p.BusinessIncubatorId AS IncubatorId,
        pu.Role AS Role,
        1 AS IsActive,
        '2024-01-01 00:00:00' AS LastSyncedAt
    FROM [businessincubators].[ProjectUsers] pu
    INNER JOIN [businessincubators].[Projects] p ON pu.ProjectId = p.Id
    INNER JOIN [dbo].[AspNetUsers] u ON pu.UserId = u.Id
    WHERE pu.IsActive = 1
      -- Sync all demo users to their respective incubators
      AND pu.UserId IN (@DemoStarterId, @DemoMentorId, @DemoCoordinatorId)
      AND p.BusinessIncubatorId = @DemoIncubatorId
) AS source ON target.UserId = source.UserId AND target.IncubatorId = source.IncubatorId
WHEN MATCHED THEN
    UPDATE SET 
        Role = source.Role,
        IsActive = source.IsActive,
        LastSyncedAt = source.LastSyncedAt
WHEN NOT MATCHED THEN
    INSERT ([UserId], [IncubatorId], [Role], [IsActive], [LastSyncedAt])
    VALUES (source.UserId, source.IncubatorId, source.Role, source.IsActive, source.LastSyncedAt);

-- Add demo admin to demo incubator only
IF @DemoAdminId IS NOT NULL AND @DemoIncubatorId IS NOT NULL
BEGIN
    MERGE [dbo].[UserIncubatorAccess] AS target
    USING (
        SELECT 
            @DemoAdminId AS UserId,
            @DemoIncubatorId AS IncubatorId,
            'Administrator' AS Role,
            1 AS IsActive,
            '2024-01-01 00:00:00' AS LastSyncedAt
    ) AS source ON target.UserId = source.UserId AND target.IncubatorId = source.IncubatorId
    WHEN MATCHED THEN
        UPDATE SET 
            Role = source.Role,
            IsActive = source.IsActive,
            LastSyncedAt = source.LastSyncedAt
    WHEN NOT MATCHED THEN
        INSERT ([UserId], [IncubatorId], [Role], [IsActive], [LastSyncedAt])
        VALUES (source.UserId, source.IncubatorId, source.Role, source.IsActive, source.LastSyncedAt);
END

DECLARE @IncubatorAccessCount INT = @@ROWCOUNT;
PRINT '[007.SeedAuthAccessTables.sql] Inserted ' + CAST(@IncubatorAccessCount AS NVARCHAR(10)) + ' UserIncubatorAccess records';

-- =============================================
-- UserMentorshipAccess: Mirror ProjectMentorAssignments
-- =============================================
PRINT '[007.SeedAuthAccessTables.sql] Seeding UserMentorshipAccess from ProjectMentorAssignments';

-- Only clear demo mentorship relationships
DELETE FROM [dbo].[UserMentorshipAccess]
WHERE MentorUserId IN (@DemoMentorId)
   OR StarterUserId IN (@DemoStarterId)
   OR ProjectId = @DemoProjectId
   OR IncubatorId = @DemoIncubatorId;

-- Use MERGE for safe mentorship sync (demo relationships only)
MERGE [dbo].[UserMentorshipAccess] AS target
USING (
    SELECT 
        pma.MentorUserId,
        pma.StarterUserId,
        pma.ProjectId,
        p.BusinessIncubatorId AS IncubatorId,
        CASE WHEN pma.Status = 'active' THEN 1 ELSE 0 END AS IsActive,
        pma.AssignedDate AS AssignedAt,
        '2024-01-01 00:00:00' AS LastSyncedAt
    FROM [businessincubators].[ProjectMentorAssignments] pma
    INNER JOIN [businessincubators].[Projects] p ON pma.ProjectId = p.Id
    INNER JOIN [dbo].[AspNetUsers] mentor ON pma.MentorUserId = mentor.Id
    INNER JOIN [dbo].[AspNetUsers] starter ON pma.StarterUserId = starter.Id
    WHERE pma.Status IN ('active', 'completed')
      -- Only sync demo mentorship relationships
      AND (pma.MentorUserId = @DemoMentorId OR pma.StarterUserId = @DemoStarterId OR pma.ProjectId = @DemoProjectId)
) AS source ON target.MentorUserId = source.MentorUserId 
            AND target.StarterUserId = source.StarterUserId 
            AND target.ProjectId = source.ProjectId
WHEN MATCHED THEN
    UPDATE SET 
        IncubatorId = source.IncubatorId,
        IsActive = source.IsActive,
        AssignedAt = source.AssignedAt,
        LastSyncedAt = source.LastSyncedAt
WHEN NOT MATCHED THEN
    INSERT ([MentorUserId], [StarterUserId], [ProjectId], [IncubatorId], [IsActive], [AssignedAt], [LastSyncedAt])
    VALUES (source.MentorUserId, source.StarterUserId, source.ProjectId, source.IncubatorId, source.IsActive, source.AssignedAt, source.LastSyncedAt);

DECLARE @MentorshipAccessCount INT = @@ROWCOUNT;
PRINT '[007.SeedAuthAccessTables.sql] Inserted ' + CAST(@MentorshipAccessCount AS NVARCHAR(10)) + ' UserMentorshipAccess records';

-- =============================================
-- Verification Queries
-- =============================================
PRINT '[007.SeedAuthAccessTables.sql] Verification:';

-- Count demo ProjectUsers vs UserProjectAccess
DECLARE @DemoProjectUsersCount INT = (
    SELECT COUNT(*) FROM [businessincubators].[ProjectUsers] pu
    INNER JOIN [businessincubators].[Projects] p ON pu.ProjectId = p.Id
    WHERE pu.IsActive = 1
      AND (pu.UserId IN (@DemoStarterId, @DemoMentorId, @DemoCoordinatorId, @DemoAdminId)
           OR pu.ProjectId = @DemoProjectId
           OR p.BusinessIncubatorId = @DemoIncubatorId)
);
DECLARE @DemoUserProjectAccessCount INT = (
    SELECT COUNT(*) FROM [dbo].[UserProjectAccess]
    WHERE IsActive = 1
      AND (UserId IN (@DemoStarterId, @DemoMentorId, @DemoCoordinatorId, @DemoAdminId)
           OR ProjectId = @DemoProjectId
           OR IncubatorId = @DemoIncubatorId)
);
PRINT '[007.SeedAuthAccessTables.sql] Demo ProjectUsers: ' + CAST(@DemoProjectUsersCount AS NVARCHAR(10)) + ', Demo UserProjectAccess: ' + CAST(@DemoUserProjectAccessCount AS NVARCHAR(10));

-- Count demo mentorship assignments
DECLARE @DemoMentorAssignmentsCount INT = (
    SELECT COUNT(*) FROM [businessincubators].[ProjectMentorAssignments]
    WHERE Status IN ('active', 'completed')
      AND (MentorUserId = @DemoMentorId OR StarterUserId = @DemoStarterId OR ProjectId = @DemoProjectId)
);
DECLARE @DemoUserMentorshipAccessCount INT = (
    SELECT COUNT(*) FROM [dbo].[UserMentorshipAccess]
    WHERE MentorUserId = @DemoMentorId OR StarterUserId = @DemoStarterId OR ProjectId = @DemoProjectId
);
PRINT '[007.SeedAuthAccessTables.sql] Demo ProjectMentorAssignments: ' + CAST(@DemoMentorAssignmentsCount AS NVARCHAR(10)) + ', Demo UserMentorshipAccess: ' + CAST(@DemoUserMentorshipAccessCount AS NVARCHAR(10));

-- List demo data for verification
PRINT '[007.SeedAuthAccessTables.sql] Demo UserProjectAccess records:';
SELECT
    upa.UserId,
    u.UserName,
    upa.ProjectId,
    upa.IncubatorId,
    upa.Role,
    upa.IsActive
FROM [dbo].[UserProjectAccess] upa
INNER JOIN [AspNetUsers] u ON upa.UserId = u.Id
WHERE upa.UserId IN (@DemoStarterId, @DemoMentorId, @DemoCoordinatorId, @DemoAdminId)
   OR upa.ProjectId = @DemoProjectId
   OR upa.IncubatorId = @DemoIncubatorId
ORDER BY upa.Id;

PRINT '[007.SeedAuthAccessTables.sql] Finished successfully (only demo data affected)';