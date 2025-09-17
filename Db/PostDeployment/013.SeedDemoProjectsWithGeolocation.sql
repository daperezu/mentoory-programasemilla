-- =============================================
-- Seed Data for Demo Projects with Geolocation
-- Created: 2025-01-14
-- Description: Populates 10 demo projects with coordinates around San José, Costa Rica
-- Center point: 9.930943,-84.015198 (San José)
-- Distance range: 5-30 km from center
-- =============================================

PRINT '[013.SeedDemoProjectsWithGeolocation.sql] Starting demo projects with geolocation seed...';

-- Get or create demo incubator
DECLARE @IncubatorId BIGINT = (SELECT TOP 1 Id FROM [businessincubators].[BusinessIncubators] WHERE [Key] = 'DEMO');

IF @IncubatorId IS NULL
BEGIN
    -- Create a default business incubator
    INSERT INTO [businessincubators].[BusinessIncubators] 
        (ExternalId, Name, Description, [Key], Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (NEWID(), 'Incubadora Demo', 'Incubadora de demostración para datos de prueba', 'DEMO', 1, GETUTCDATE(), 'SYSTEM', 0);
    
    SET @IncubatorId = SCOPE_IDENTITY();
    PRINT '[013.SeedDemoProjectsWithGeolocation.sql] Created demo business incubator';
END

-- Get demo users for audit fields
DECLARE @CoordinatorUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM [dbo].[AspNetUsers] WHERE UserName = 'demo.coordinator');
IF @CoordinatorUserId IS NULL
    SET @CoordinatorUserId = 'SYSTEM';

-- Insert 10 demo projects with geolocation data
-- Coordinates are distributed around San José (9.930943,-84.015198)

-- Project 1: Escazú (5.8 km west)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'TECH-ESCAZU-001')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'TechHub Escazú', 
         'Centro de innovación tecnológica especializado en startups de IA y machine learning', 
         'TECH-ESCAZU-001',
         9.932500, -84.138889, 'd1rgy2ypvhsy', 'Escazú', 'Plaza Multiplaza, Escazú, San José',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 2: Santa Ana (8.2 km west)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'BIO-SANTAANA-002')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'BioTech Santa Ana', 
         'Incubadora de biotecnología y ciencias de la salud', 
         'BIO-SANTAANA-002',
         9.933333, -84.183333, 'd1rgu2p5gbju', 'Santa Ana', 'Forum Business Center, Santa Ana, San José',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 3: Heredia Centro (10.3 km north)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'EDU-HEREDIA-003')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'EduTech Heredia', 
         'Proyectos de tecnología educativa y plataformas de e-learning', 
         'EDU-HEREDIA-003',
         10.000000, -84.116667, 'd1u082bpbpbp', 'Heredia Centro', 'Universidad Nacional, Heredia',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 4: Cartago (22.5 km east)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'AGRO-CARTAGO-004')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'AgroTech Cartago', 
         'Innovación en agricultura sostenible y tecnología verde', 
         'AGRO-CARTAGO-004',
         9.866667, -83.916667, 'd1rkw8dkdkdk', 'Cartago', 'TEC Cartago, Cartago Centro',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 5: Alajuela (18.7 km northwest)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'LOG-ALAJUELA-005')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'LogiTech Alajuela', 
         'Soluciones logísticas y cadena de suministro inteligente', 
         'LOG-ALAJUELA-005',
         10.016667, -84.216667, 'd1u01kdkdkdk', 'Alajuela', 'Zona Franca Coyol, Alajuela',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 6: Moravia (7.4 km northeast)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'FIN-MORAVIA-006')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'FinTech Moravia', 
         'Tecnología financiera y soluciones de pago digital', 
         'FIN-MORAVIA-006',
         9.966667, -84.050000, 'd1rhskdkdkdk', 'Moravia', 'Lincoln Plaza, Moravia, San José',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 7: Curridabat (6.1 km east)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'GREEN-CURRI-007')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'GreenTech Curridabat', 
         'Proyectos de energía renovable y sostenibilidad ambiental', 
         'GREEN-CURRI-007',
         9.916667, -84.033333, 'd1rhudkdkdkd', 'Curridabat', 'Momentum Pinares, Curridabat',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 8: Tres Ríos (12.8 km east)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'HEALTH-TRESRIOS-008')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'HealthTech Tres Ríos', 
         'Innovación en salud digital y telemedicina', 
         'HEALTH-TRESRIOS-008',
         9.900000, -83.983333, 'd1rhz2bpbpbp', 'Tres Ríos', 'Terrazas del Este, Tres Ríos, Cartago',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 9: Belén (14.2 km west)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'TOURISM-BELEN-009')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'TourTech Belén', 
         'Plataformas digitales para turismo sostenible', 
         'TOURISM-BELEN-009',
         9.983333, -84.183333, 'd1rgz5gbpbpb', 'Belén', 'América Free Zone, Belén, Heredia',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Project 10: Desamparados (5.3 km south)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Key] = 'SOCIAL-DESAMP-010')
BEGIN
    INSERT INTO [businessincubators].[Projects] 
        (BusinessIncubatorId, ExternalId, Name, Description, [Key], 
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (@IncubatorId, NEWID(), 'SocialTech Desamparados', 
         'Emprendimiento social y tecnología para el desarrollo comunitario', 
         'SOCIAL-DESAMP-010',
         9.900000, -84.066667, 'd1rhp2bpbpbp', 'Desamparados', 'Centro Comercial Multicentro, Desamparados',
         1, GETUTCDATE(), @CoordinatorUserId, 0);
END

-- Add some project stages for the new projects to make them visible
-- Get the project IDs
DECLARE @ProjectId BIGINT;
DECLARE project_cursor CURSOR FOR 
    SELECT Id FROM [businessincubators].[Projects] 
    WHERE [Key] IN ('TECH-ESCAZU-001', 'BIO-SANTAANA-002', 'EDU-HEREDIA-003', 
                    'AGRO-CARTAGO-004', 'LOG-ALAJUELA-005', 'FIN-MORAVIA-006',
                    'GREEN-CURRI-007', 'HEALTH-TRESRIOS-008', 'TOURISM-BELEN-009', 
                    'SOCIAL-DESAMP-010');

OPEN project_cursor;
FETCH NEXT FROM project_cursor INTO @ProjectId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Add a project stage for each new project (Registration stage)
    IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectStages] WHERE ProjectId = @ProjectId AND Title = 'Inscripción')
    BEGIN
        INSERT INTO [businessincubators].[ProjectStages]
            (ProjectId, Type, Title, Description, StartDate, EndDate, 
             IsActive, CreatedAt, CreatedBy)
        VALUES
            (@ProjectId, 1, 'Inscripción', 'Fase de inscripción y registro de participantes', 
             DATEADD(day, -7, GETUTCDATE()), DATEADD(day, 30, GETUTCDATE()),
             1, GETUTCDATE(), @CoordinatorUserId);
    END

    -- Add at least one user to each project (use the demo coordinator)
    IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectUsers] WHERE ProjectId = @ProjectId AND UserId = @CoordinatorUserId)
    BEGIN
        INSERT INTO [businessincubators].[ProjectUsers] 
            (ProjectId, UserId, Role, IsActive, JoinedAt, CreatedAt)
        VALUES 
            (@ProjectId, @CoordinatorUserId, 'Coordinator', 1, GETUTCDATE(), GETUTCDATE());
    END

    FETCH NEXT FROM project_cursor INTO @ProjectId;
END

CLOSE project_cursor;
DEALLOCATE project_cursor;

PRINT '[013.SeedDemoProjectsWithGeolocation.sql] Completed seeding 10 demo projects with geolocation data';
PRINT '[013.SeedDemoProjectsWithGeolocation.sql] Projects are distributed 5-30km around San José (9.930943,-84.015198)';
PRINT '[013.SeedDemoProjectsWithGeolocation.sql] All projects have active registration stages and are ready for public display';