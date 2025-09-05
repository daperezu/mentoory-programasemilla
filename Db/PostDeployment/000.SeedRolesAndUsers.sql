-- ==========================================================================================
-- Post-Deployment Script for Seeding LinaSys Roles and Admin User
-- ==========================================================================================

-- Insert or Update Roles Using MERGE (Never delete roles to preserve custom ones)
MERGE INTO AspNetRoles AS target
USING (VALUES 
    ('Global Administrator', 'GLOBALADMINISTRATOR'),
    ('Administrator', 'ADMINISTRATOR'),
    ('Coordinator', 'COORDINATOR'),
    ('Guide', 'GUIDE'),
    ('Mentor', 'MENTOR'),
    ('Facilitator', 'FACILITATOR'),
    ('Starter', 'STARTER'),
    ('Liaison', 'LIAISON')
) AS source (RoleName, NormalizedRoleName)
ON target.Name = source.RoleName
WHEN MATCHED THEN
    UPDATE SET NormalizedName = source.NormalizedRoleName
WHEN NOT MATCHED THEN
    INSERT (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), source.RoleName, source.NormalizedRoleName, NEWID())

; -- Sepparator semicolon after MERGE statement

-- Ensure Default Admin User Exists
DECLARE @AdminUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = '00119922883377446655');

IF @AdminUserId IS NULL
BEGIN
    SET @AdminUserId = NEWID(); -- Generate a unique ID
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
        PasswordHash, 
        SecurityStamp, ConcurrencyStamp, AccessFailedCount, LockoutEnabled, 
        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled
    )
    VALUES (
        @AdminUserId, '00119922883377446655', '00119922883377446655', 'globaladmin@mentoory.com', 'GLOBALADMIN@MENTOORY.COM', 1, 
        'AQAAAAIAAYagAAAAELtrv8uTJ3doJ0w6TF1NjAb/1opJilLBd3Hk1FpVpgmSpkg4lDkoBQ6SLww8EOfMag==', -- adminlinasys!0
        'XDXFG5AKFNQM3QRBXIIZKM3AWS2SWFS2', '0eede1d6-fb77-47a6-8987-0cd77abbacc2', 0, 1,
        NULL, 0, 0
    );
END
ELSE
BEGIN
    SET @AdminUserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = '00119922883377446655');
END

-- Ensure Main Developer User Exists
DECLARE @MainDeveloperUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = '303830004');

IF @MainDeveloperUserId IS NULL
BEGIN
    SET @MainDeveloperUserId = NEWID(); -- Generate a unique ID
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
        PasswordHash, 
        SecurityStamp, ConcurrencyStamp, AccessFailedCount, LockoutEnabled, 
        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled
    )
    VALUES (
        @MainDeveloperUserId, '303830004', '303830004', 'danny.perez.u@gmail.com', 'DANNY.PEREZ.U@GMAIL.COM', 1, 
        'AQAAAAIAAYagAAAAEHPpTthM1IL+SpKJnP54JifeHGN0QaqSP+oK/hp3eKbhRMBlLyn20b2FhZL/8UoVMw==', -- Nvxcrsm19!
        'JCMXF47PDD75LJFQMMJV2VBN7NBB2PDK', 'c9876b9b-aa1d-46c0-8897-eb827a38168e', 0, 1,
        NULL, 0, 0
    );
END
ELSE
BEGIN
    SET @MainDeveloperUserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = '303830004');
END

-- Ensure Demo Starter User Exists
DECLARE @DemoStarterUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.starter');

IF @DemoStarterUserId IS NULL
BEGIN
    SET @DemoStarterUserId = NEWID();
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
        PasswordHash, 
        SecurityStamp, ConcurrencyStamp, AccessFailedCount, LockoutEnabled, 
        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled
    )
    VALUES (
        @DemoStarterUserId, 'demo.starter', 'DEMO.STARTER', 'starter@demo.com', 'STARTER@DEMO.COM', 1, 
        'AQAAAAIAAYagAAAAEHPpTthM1IL+SpKJnP54JifeHGN0QaqSP+oK/hp3eKbhRMBlLyn20b2FhZL/8UoVMw==', -- Nvxcrsm19!
        'JCMXF47PDD75LJFQMMJV2VBN7NBB2PDK', 'c9876b9b-aa1d-46c0-8897-eb827a38168e', 0, 1,
        NULL, 0, 0
    );
END
ELSE
BEGIN
    SET @DemoStarterUserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.starter');
END

-- Ensure Demo Mentor User Exists
DECLARE @DemoMentorUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.mentor');

IF @DemoMentorUserId IS NULL
BEGIN
    SET @DemoMentorUserId = NEWID();
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
        PasswordHash, 
        SecurityStamp, ConcurrencyStamp, AccessFailedCount, LockoutEnabled, 
        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled
    )
    VALUES (
        @DemoMentorUserId, 'demo.mentor', 'DEMO.MENTOR', 'mentor@demo.com', 'MENTOR@DEMO.COM', 1, 
        'AQAAAAIAAYagAAAAEHPpTthM1IL+SpKJnP54JifeHGN0QaqSP+oK/hp3eKbhRMBlLyn20b2FhZL/8UoVMw==', -- Nvxcrsm19!
        'JCMXF47PDD75LJFQMMJV2VBN7NBB2PDK', 'c9876b9b-aa1d-46c0-8897-eb827a38168e', 0, 1,
        NULL, 0, 0
    );
END
ELSE
BEGIN
    SET @DemoMentorUserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.mentor');
END

-- Ensure Demo Coordinator User Exists
DECLARE @DemoCoordinatorUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.coordinator');

IF @DemoCoordinatorUserId IS NULL
BEGIN
    SET @DemoCoordinatorUserId = NEWID();
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
        PasswordHash, 
        SecurityStamp, ConcurrencyStamp, AccessFailedCount, LockoutEnabled, 
        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled
    )
    VALUES (
        @DemoCoordinatorUserId, 'demo.coordinator', 'DEMO.COORDINATOR', 'coordinator@demo.com', 'COORDINATOR@DEMO.COM', 1, 
        'AQAAAAIAAYagAAAAEHPpTthM1IL+SpKJnP54JifeHGN0QaqSP+oK/hp3eKbhRMBlLyn20b2FhZL/8UoVMw==', -- Nvxcrsm19!
        'JCMXF47PDD75LJFQMMJV2VBN7NBB2PDK', 'c9876b9b-aa1d-46c0-8897-eb827a38168e', 0, 1,
        NULL, 0, 0
    );
END
ELSE
BEGIN
    SET @DemoCoordinatorUserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.coordinator');
END

-- Assign Roles using MERGE
DECLARE @AdminRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Global Administrator');
DECLARE @StarterRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Starter');
DECLARE @MentorRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Mentor');
DECLARE @CoordinatorRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Coordinator');

MERGE INTO AspNetUserRoles AS target
USING (
    SELECT @AdminUserId AS UserId, @AdminRoleId AS RoleId
    UNION ALL
    SELECT @MainDeveloperUserId AS UserId, @AdminRoleId AS RoleId
    UNION ALL
    SELECT @DemoStarterUserId AS UserId, @StarterRoleId AS RoleId
    UNION ALL
    SELECT @DemoMentorUserId AS UserId, @MentorRoleId AS RoleId
    UNION ALL
    SELECT @DemoCoordinatorUserId AS UserId, @CoordinatorRoleId AS RoleId
) AS source
ON target.UserId = source.UserId AND target.RoleId = source.RoleId
WHEN NOT MATCHED THEN
    INSERT (UserId, RoleId) VALUES (source.UserId, source.RoleId);

; -- Sepparator semicolon after MERGE statement

-- Ensure User Profiles Exist in new UserManagement schema
IF NOT EXISTS (SELECT 1 FROM [usermanagement].[UserProfiles] WHERE UserId = @AdminUserId)
BEGIN
    INSERT INTO [usermanagement].[UserProfiles] (UserId, FirstName, LastName, Identification, IsActive, CreatedAt, CreatedBy) 
    VALUES (@AdminUserId, 'Global Admin', 'User', '00119922883377446655', 1, GETUTCDATE(), 'SYSTEM');
END

IF NOT EXISTS (SELECT 1 FROM [usermanagement].[UserProfiles] WHERE UserId = @MainDeveloperUserId)
BEGIN
    INSERT INTO [usermanagement].[UserProfiles] (UserId, FirstName, LastName, Identification, IsActive, CreatedAt, CreatedBy) 
    VALUES (@MainDeveloperUserId, 'Danny', 'Pérez Umaña', '303830004', 1, GETUTCDATE(), 'SYSTEM');
END

IF NOT EXISTS (SELECT 1 FROM [usermanagement].[UserProfiles] WHERE UserId = @DemoStarterUserId)
BEGIN
    INSERT INTO [usermanagement].[UserProfiles] (UserId, FirstName, LastName, Identification, IsActive, CreatedAt, CreatedBy) 
    VALUES (@DemoStarterUserId, 'Starter', 'Demo', 'DEMO-STARTER-001', 1, GETUTCDATE(), 'SYSTEM');
END

IF NOT EXISTS (SELECT 1 FROM [usermanagement].[UserProfiles] WHERE UserId = @DemoMentorUserId)
BEGIN
    INSERT INTO [usermanagement].[UserProfiles] (UserId, FirstName, LastName, Identification, IsActive, CreatedAt, CreatedBy) 
    VALUES (@DemoMentorUserId, 'Mentor', 'Demo', 'DEMO-MENTOR-001', 1, GETUTCDATE(), 'SYSTEM');
END

IF NOT EXISTS (SELECT 1 FROM [usermanagement].[UserProfiles] WHERE UserId = @DemoCoordinatorUserId)
BEGIN
    INSERT INTO [usermanagement].[UserProfiles] (UserId, FirstName, LastName, Identification, IsActive, CreatedAt, CreatedBy) 
    VALUES (@DemoCoordinatorUserId, 'Coordinador', 'Demo', 'DEMO-COORD-001', 1, GETUTCDATE(), 'SYSTEM');
END
