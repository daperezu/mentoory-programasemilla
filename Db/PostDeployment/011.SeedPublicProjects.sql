-- =============================================
-- Seed data for public projects with stages
-- REQ-012: Phoenix Homepage Redesign
-- =============================================

PRINT '[011.SeedPublicProjects.sql] Starting public projects seed data';

-- Exit if data already exists to avoid duplicates
IF EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'EcoTech Solutions')
BEGIN
    PRINT '[011.SeedPublicProjects.sql] Public projects already seeded, skipping...';
    RETURN;
END

-- Variables for incubator and project IDs
DECLARE @PublicIncubatorId1 BIGINT;
DECLARE @PublicIncubatorId2 BIGINT;
DECLARE @PublicProjectId BIGINT;
DECLARE @PublicCurrentDate DATETIME2 = GETUTCDATE();
DECLARE @PublicCounter INT = 1;

-- Get or create sample incubators
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[BusinessIncubators] WHERE [Name] = 'Incubadora TEC San José')
BEGIN
    INSERT INTO [businessincubators].[BusinessIncubators]
        ([ExternalId], [Name], [Description], [Key], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (NEWID(), 'Incubadora TEC San José', 'Centro de emprendimiento e innovación del Tecnológico de Costa Rica',
         'incubadora-tec-san-jose', 1, @PublicCurrentDate, 'system');
    SET @PublicIncubatorId1 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @PublicIncubatorId1 = [Id] FROM [businessincubators].[BusinessIncubators] WHERE [Name] = 'Incubadora TEC San José';
END

IF NOT EXISTS (SELECT 1 FROM [businessincubators].[BusinessIncubators] WHERE [Name] = 'Auge UCR')
BEGIN
    INSERT INTO [businessincubators].[BusinessIncubators]
        ([ExternalId], [Name], [Description], [Key], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (NEWID(), 'Auge UCR', 'Agencia Universitaria para la Gestión del Emprendimiento',
         'auge-ucr', 1, @PublicCurrentDate, 'system');
    SET @PublicIncubatorId2 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @PublicIncubatorId2 = [Id] FROM [businessincubators].[BusinessIncubators] WHERE [Name] = 'Auge UCR';
END

-- Create sample projects with varied dates across next 60 days
-- Project 1: Starting soon (in 3 days)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'EcoTech Solutions')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId1, NEWID(), 'EcoTech Solutions',
         'Plataforma digital para gestión de residuos y reciclaje inteligente en comunidades urbanas',
         'ECOTECH2024', 9.9347, -84.0875, 'd1u0tb', 'San José Centro', 'Avenida Central, San José',
         'projects/ecotech-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    -- Add stages
    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Inscripción Abierta', 'Período de registro para emprendedores interesados',
         DATEADD(DAY, 3, @PublicCurrentDate), DATEADD(DAY, 10, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 2, 'Evaluación Inicial', 'Revisión de propuestas y selección de participantes',
         DATEADD(DAY, 11, @PublicCurrentDate), DATEADD(DAY, 20, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 3, 'Mentoría Intensiva', 'Sesiones de trabajo con mentores especializados',
         DATEADD(DAY, 21, @PublicCurrentDate), DATEADD(DAY, 45, @PublicCurrentDate), 0, @PublicCurrentDate, 'system');
END

-- Project 2: Starting next week
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'AgroSmart CR')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId2, NEWID(), 'AgroSmart CR',
         'Tecnología IoT para optimización de cultivos y agricultura de precisión',
         'AGRO2024', 9.9707, -84.1310, 'd1u0vw', 'San Pedro', 'Ciudad Universitaria Rodrigo Facio',
         'projects/agrosmart-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Convocatoria', 'Apertura de inscripciones para agricultores innovadores',
         DATEADD(DAY, 7, @PublicCurrentDate), DATEADD(DAY, 14, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 2, 'Bootcamp Inicial', 'Formación intensiva en tecnología agrícola',
         DATEADD(DAY, 15, @PublicCurrentDate), DATEADD(DAY, 25, @PublicCurrentDate), 1, @PublicCurrentDate, 'system');
END

-- Project 3: Starting in 2 weeks
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'FinTech Innova')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId1, NEWID(), 'FinTech Innova',
         'Desarrollo de soluciones financieras digitales para PYMES costarricenses',
         'FINTECH2024', 9.9281, -84.0907, 'd1u0sy', 'Escazú', 'Centro Corporativo Plaza Roble',
         'projects/fintech-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Pre-registro', 'Fase de interés y documentación inicial',
         DATEADD(DAY, 14, @PublicCurrentDate), DATEADD(DAY, 21, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 4, 'Pitch Day', 'Presentación de proyectos ante inversionistas',
         DATEADD(DAY, 50, @PublicCurrentDate), DATEADD(DAY, 51, @PublicCurrentDate), 0, @PublicCurrentDate, 'system');
END

-- Project 4: Starting tomorrow (urgent)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'Salud Digital 360')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId2, NEWID(), 'Salud Digital 360',
         'Telemedicina y soluciones digitales para el sector salud',
         'SALUD2024', 9.8650, -83.9150, 'd1u60b', 'Cartago Centro', 'Hospital Max Peralta',
         'projects/salud-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Inscripción Express', 'Última oportunidad para registrarse',
         DATEADD(DAY, 1, @PublicCurrentDate), DATEADD(DAY, 3, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 2, 'Formación Rápida', 'Capacitación acelerada en salud digital',
         DATEADD(DAY, 4, @PublicCurrentDate), DATEADD(DAY, 10, @PublicCurrentDate), 1, @PublicCurrentDate, 'system');
END

-- Project 5: Starting in 1 month
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'TurismoTech Costa Rica')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId1, NEWID(), 'TurismoTech Costa Rica',
         'Innovación tecnológica para el sector turístico nacional',
         'TURISMO2024', 10.0160, -84.2163, 'd1u35x', 'Alajuela', 'Aeropuerto Juan Santamaría',
         'projects/turismo-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Apertura de Inscripciones', 'Convocatoria para emprendedores del sector turismo',
         DATEADD(DAY, 30, @PublicCurrentDate), DATEADD(DAY, 40, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 3, 'Aceleración', 'Programa intensivo de desarrollo de negocios',
         DATEADD(DAY, 41, @PublicCurrentDate), DATEADD(DAY, 90, @PublicCurrentDate), 0, @PublicCurrentDate, 'system');
END

-- Project 6: Without geolocation (remote)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'EdTech Virtual')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [LocationName], [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId2, NEWID(), 'EdTech Virtual',
         'Plataformas educativas innovadoras para el aprendizaje en línea',
         'EDTECH2024', 'Virtual - Toda Costa Rica',
         'projects/edtech-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Registro Virtual', 'Inscripciones 100% en línea',
         DATEADD(DAY, 5, @PublicCurrentDate), DATEADD(DAY, 15, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 2, 'Talleres Virtuales', 'Capacitación remota en pedagogía digital',
         DATEADD(DAY, 16, @PublicCurrentDate), DATEADD(DAY, 30, @PublicCurrentDate), 1, @PublicCurrentDate, 'system');
END

-- Project 7: Starting in 45 days
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'GreenEnergy CR')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId1, NEWID(), 'GreenEnergy CR',
         'Soluciones de energía renovable para hogares y empresas',
         'GREEN2024', 9.7489, -83.7534, 'd1u9pc', 'Turrialba', 'CATIE',
         'projects/green-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Convocatoria Abierta', 'Registro para proyectos de energía limpia',
         DATEADD(DAY, 45, @PublicCurrentDate), DATEADD(DAY, 55, @PublicCurrentDate), 1, @PublicCurrentDate, 'system');
END

-- Project 8: Starting next month
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'FoodTech Innovation')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId2, NEWID(), 'FoodTech Innovation',
         'Tecnología aplicada a la industria alimentaria y gastronomía',
         'FOOD2024', 9.9983, -84.1117, 'd1u0xm', 'Heredia Centro', 'Paseo de las Flores',
         'projects/foodtech-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Pre-inscripción', 'Fase inicial de registro',
         DATEADD(DAY, 25, @PublicCurrentDate), DATEADD(DAY, 35, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 2, 'Bootcamp Gastronómico', 'Inmersión en innovación culinaria',
         DATEADD(DAY, 36, @PublicCurrentDate), DATEADD(DAY, 42, @PublicCurrentDate), 1, @PublicCurrentDate, 'system');
END

-- Project 9: Starting in 2 months
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'SmartCity Guanacaste')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId1, NEWID(), 'SmartCity Guanacaste',
         'Soluciones inteligentes para ciudades sostenibles en Guanacaste',
         'SMART2024', 10.6345, -85.4407, 'd1t8hq', 'Liberia', 'Aeropuerto Daniel Oduber',
         'projects/smartcity-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Inscripciones', 'Convocatoria regional Guanacaste',
         DATEADD(DAY, 60, @PublicCurrentDate), DATEADD(DAY, 75, @PublicCurrentDate), 1, @PublicCurrentDate, 'system');
END

-- Project 10: Starting in 1 week (with multiple stages)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[Projects] WHERE [Name] = 'BioTech Costa Rica')
BEGIN
    INSERT INTO [businessincubators].[Projects]
        ([BusinessIncubatorId], [ExternalId], [Name], [Description], [Key],
         [Latitude], [Longitude], [Geohash], [LocationName], [LocationAddress],
         [HeroImageBlobId], [HasHeroImage], [Status], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicIncubatorId2, NEWID(), 'BioTech Costa Rica',
         'Biotecnología aplicada a medicina y agricultura',
         'BIO2024', 9.9374, -84.0513, 'd1u0tx', 'San José', 'Ciudad de la Investigación UCR',
         'projects/biotech-hero.jpg', 1, 1, @PublicCurrentDate, 'system');

    SET @PublicProjectId = SCOPE_IDENTITY();

    INSERT INTO [businessincubators].[ProjectStages]
        ([ProjectId], [Type], [Title], [Description], [StartDate], [EndDate], [IsActive], [CreatedAt], [CreatedBy])
    VALUES
        (@PublicProjectId, 1, 'Registro Abierto', 'Inscripciones para científicos emprendedores',
         DATEADD(DAY, 7, @PublicCurrentDate), DATEADD(DAY, 14, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 2, 'Evaluación Técnica', 'Revisión de propuestas biotecnológicas',
         DATEADD(DAY, 15, @PublicCurrentDate), DATEADD(DAY, 21, @PublicCurrentDate), 1, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 3, 'Laboratorio de Ideas', 'Desarrollo de prototipos',
         DATEADD(DAY, 22, @PublicCurrentDate), DATEADD(DAY, 50, @PublicCurrentDate), 0, @PublicCurrentDate, 'system'),
        (@PublicProjectId, 4, 'Demo Day', 'Presentación a inversionistas',
         DATEADD(DAY, 51, @PublicCurrentDate), DATEADD(DAY, 52, @PublicCurrentDate), 0, @PublicCurrentDate, 'system');
END

-- Add some sample project users for active participant counts
DECLARE @UserId NVARCHAR(450);
SET @UserId = (SELECT TOP 1 [Id] FROM [dbo].[AspNetUsers] WHERE [UserName] = 'demo.starter');

IF @UserId IS NOT NULL
BEGIN
    -- Add user to first 5 projects as participant
    DECLARE @ProjectCursor CURSOR;
    SET @ProjectCursor = CURSOR FOR
        SELECT TOP 5 [Id] FROM [businessincubators].[Projects] ORDER BY [CreatedAt] DESC;

    OPEN @ProjectCursor;
    FETCH NEXT FROM @ProjectCursor INTO @PublicProjectId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectUsers]
                      WHERE [ProjectId] = @PublicProjectId AND [UserId] = @UserId)
        BEGIN
            INSERT INTO [businessincubators].[ProjectUsers]
                ([ProjectId], [UserId], [Role], [IsActive], [JoinedAt], [CreatedAt])
            VALUES
                (@PublicProjectId, @UserId, 'Participante', 1, @PublicCurrentDate, @PublicCurrentDate);
        END

        FETCH NEXT FROM @ProjectCursor INTO @PublicProjectId;
    END

    CLOSE @ProjectCursor;
    DEALLOCATE @ProjectCursor;
END

PRINT '[011.SeedPublicProjects.sql] Completed seeding public projects with stages';
GO