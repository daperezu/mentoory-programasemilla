-- Drop the Auth UserProfiles table as it's no longer used
-- UserManagement domain now owns user profile data

IF OBJECT_ID('dbo.UserProfiles', 'U') IS NOT NULL
BEGIN
    PRINT 'Dropping obsolete dbo.UserProfiles table...';
    DROP TABLE dbo.UserProfiles;
    PRINT 'Table dbo.UserProfiles dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.UserProfiles does not exist - nothing to drop.';
END