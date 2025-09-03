-- =============================================
-- Seed Data for Auth Domain Access Tables
-- Created: 2025-01-13
-- Description: Mirrors existing relationships from BusinessIncubator domain into Auth read models
-- This ensures Auth services have access control data on fresh install
-- =============================================

PRINT '[007.SeedAuthAccessTables.sql] Starting';

-- =============================================
-- UserProjectAccess: Mirror businessincubators.ProjectUsers
-- =============================================
PRINT '[007.SeedAuthAccessTables.sql] Seeding UserProjectAccess from ProjectUsers';

-- First, clear any existing data to avoid duplicates
DELETE FROM [dbo].[UserProjectAccess];

-- Insert records from businessincubators.ProjectUsers
-- Join with Projects to get the IncubatorId
-- Only insert if the user exists in AspNetUsers
INSERT INTO [dbo].[UserProjectAccess]
    ([UserId], [ProjectId], [IncubatorId], [Role], [IsActive], [LastSyncedAt])
SELECT 
    pu.UserId,
    pu.ProjectId,
    p.BusinessIncubatorId, -- Get IncubatorId from Project
    pu.Role,
    pu.IsActive,
    '2024-01-01 00:00:00' -- Seed data sync date
FROM [businessincubators].[ProjectUsers] pu
INNER JOIN [businessincubators].[Projects] p ON pu.ProjectId = p.Id
INNER JOIN [dbo].[AspNetUsers] u ON pu.UserId = u.Id -- Ensure user exists
WHERE pu.IsActive = 1;

DECLARE @ProjectAccessCount INT = @@ROWCOUNT;
PRINT '[007.SeedAuthAccessTables.sql] Inserted ' + CAST(@ProjectAccessCount AS NVARCHAR(10)) + ' UserProjectAccess records';

-- =============================================
-- UserIncubatorAccess: Create incubator-level access
-- =============================================
PRINT '[007.SeedAuthAccessTables.sql] Seeding UserIncubatorAccess';

-- Clear existing data
DELETE FROM [dbo].[UserIncubatorAccess];

-- Insert incubator access for coordinators and admins
-- Coordinators typically have incubator-level access
-- Only insert if the user exists in AspNetUsers
INSERT INTO [dbo].[UserIncubatorAccess]
    ([UserId], [IncubatorId], [Role], [IsActive], [LastSyncedAt])
SELECT DISTINCT
    pu.UserId,
    p.BusinessIncubatorId,
    'Coordinator', -- Role
    1, -- IsActive
    '2024-01-01 00:00:00' -- Seed data sync date
FROM [businessincubators].[ProjectUsers] pu
INNER JOIN [businessincubators].[Projects] p ON pu.ProjectId = p.Id
INNER JOIN [dbo].[AspNetUsers] u ON pu.UserId = u.Id -- Ensure user exists
WHERE pu.Role = 'Coordinator' AND pu.IsActive = 1;

-- Also add global administrators to all incubators
INSERT INTO [dbo].[UserIncubatorAccess]
    ([UserId], [IncubatorId], [Role], [IsActive], [LastSyncedAt])
SELECT DISTINCT
    ur.UserId,
    bi.Id,
    r.Name, -- Role from AspNetRoles
    1, -- IsActive
    '2024-01-01 00:00:00' -- Seed data sync date
FROM [AspNetUserRoles] ur
INNER JOIN [AspNetRoles] r ON ur.RoleId = r.Id
CROSS JOIN [businessincubators].[BusinessIncubators] bi
WHERE r.Name IN ('Global Administrator', 'Administrator')
AND NOT EXISTS (
    SELECT 1 FROM [dbo].[UserIncubatorAccess] uia 
    WHERE uia.UserId = ur.UserId AND uia.IncubatorId = bi.Id
);

DECLARE @IncubatorAccessCount INT = @@ROWCOUNT;
PRINT '[007.SeedAuthAccessTables.sql] Inserted ' + CAST(@IncubatorAccessCount AS NVARCHAR(10)) + ' UserIncubatorAccess records';

-- =============================================
-- UserMentorshipAccess: Mirror ProjectMentorAssignments
-- =============================================
PRINT '[007.SeedAuthAccessTables.sql] Seeding UserMentorshipAccess from ProjectMentorAssignments';

-- Clear existing data
DELETE FROM [dbo].[UserMentorshipAccess];

-- Insert mentorship relationships
-- Only insert if both mentor and starter users exist in AspNetUsers
INSERT INTO [dbo].[UserMentorshipAccess]
    ([MentorUserId], [StarterUserId], [ProjectId], [IncubatorId], [IsActive], [AssignedAt], [LastSyncedAt])
SELECT 
    pma.MentorUserId,
    pma.StarterUserId,
    pma.ProjectId,
    p.BusinessIncubatorId, -- Get IncubatorId from Project
    CASE WHEN pma.Status = 'active' THEN 1 ELSE 0 END, -- IsActive based on Status
    pma.AssignedDate, -- AssignedAt
    '2024-01-01 00:00:00' -- Seed data sync date
FROM [businessincubators].[ProjectMentorAssignments] pma
INNER JOIN [businessincubators].[Projects] p ON pma.ProjectId = p.Id
INNER JOIN [dbo].[AspNetUsers] mentor ON pma.MentorUserId = mentor.Id -- Ensure mentor exists
INNER JOIN [dbo].[AspNetUsers] starter ON pma.StarterUserId = starter.Id -- Ensure starter exists
WHERE pma.Status IN ('active', 'completed'); -- Include both active and completed mentorships

DECLARE @MentorshipAccessCount INT = @@ROWCOUNT;
PRINT '[007.SeedAuthAccessTables.sql] Inserted ' + CAST(@MentorshipAccessCount AS NVARCHAR(10)) + ' UserMentorshipAccess records';

-- =============================================
-- Verification Queries
-- =============================================
PRINT '[007.SeedAuthAccessTables.sql] Verification:';

-- Count ProjectUsers vs UserProjectAccess
DECLARE @ProjectUsersCount INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectUsers] WHERE IsActive = 1);
DECLARE @UserProjectAccessCount INT = (SELECT COUNT(*) FROM [dbo].[UserProjectAccess] WHERE IsActive = 1);
PRINT '[007.SeedAuthAccessTables.sql] ProjectUsers: ' + CAST(@ProjectUsersCount AS NVARCHAR(10)) + ', UserProjectAccess: ' + CAST(@UserProjectAccessCount AS NVARCHAR(10));

-- Count mentorship assignments
DECLARE @MentorAssignmentsCount INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectMentorAssignments] WHERE Status IN ('active', 'completed'));
DECLARE @UserMentorshipAccessCount INT = (SELECT COUNT(*) FROM [dbo].[UserMentorshipAccess]);
PRINT '[007.SeedAuthAccessTables.sql] ProjectMentorAssignments: ' + CAST(@MentorAssignmentsCount AS NVARCHAR(10)) + ', UserMentorshipAccess: ' + CAST(@UserMentorshipAccessCount AS NVARCHAR(10));

-- List some sample data for verification
PRINT '[007.SeedAuthAccessTables.sql] Sample UserProjectAccess records:';
SELECT TOP 3
    upa.UserId,
    u.UserName,
    upa.ProjectId,
    upa.IncubatorId,
    upa.Role,
    upa.IsActive
FROM [dbo].[UserProjectAccess] upa
INNER JOIN [AspNetUsers] u ON upa.UserId = u.Id
ORDER BY upa.Id;

PRINT '[007.SeedAuthAccessTables.sql] Finished successfully';