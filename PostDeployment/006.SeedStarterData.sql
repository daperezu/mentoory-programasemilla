-- =============================================
-- Seed Data for Starter Role Experience
-- Created: 2025-01-08
-- Description: Populates StarterProgress and StarterTasks tables with realistic test data
-- IMPORTANT: This script only affects demo data and is safe to run multiple times
-- =============================================

-- =============================================
-- Get Demo Users (created in 000.SeedRolesAndUsers.sql)
-- =============================================
DECLARE @StarterUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.starter');
DECLARE @MentorUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.mentor');
DECLARE @CoordinatorUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.coordinator');

-- Verify users exist
IF @StarterUserId IS NULL OR @MentorUserId IS NULL OR @CoordinatorUserId IS NULL
BEGIN
    PRINT '[004.SeedStarterData.sql] Error - Demo users not found. Please ensure 000.SeedRolesAndUsers.sql has been executed.';
    RETURN;
END

PRINT '[004.SeedStarterData.sql] Using demo users from centralized seed';

-- Use demo users for the rest of the script
DECLARE @User1Id NVARCHAR(450) = @StarterUserId;
DECLARE @User2Id NVARCHAR(450) = @MentorUserId;
DECLARE @User3Id NVARCHAR(450) = @CoordinatorUserId;
DECLARE @User4Id NVARCHAR(450) = @StarterUserId; -- Reuse for additional data
DECLARE @User5Id NVARCHAR(450) = @StarterUserId; -- Reuse for additional data

-- Check for existing projects or use safe defaults
DECLARE @Project1Id BIGINT;
DECLARE @Project2Id BIGINT;
DECLARE @Project3Id BIGINT;

-- Get the first 3 projects if they exist
SELECT TOP 1 @Project1Id = Id FROM [businessincubators].[Projects] WHERE Id = 1;
SELECT TOP 1 @Project2Id = Id FROM [businessincubators].[Projects] WHERE Id = 2;
SELECT TOP 1 @Project3Id = Id FROM [businessincubators].[Projects] WHERE Id = 3;

-- First ensure we have a business incubator
DECLARE @IncubatorId BIGINT = (SELECT TOP 1 Id FROM [businessincubators].[BusinessIncubators] WHERE [Key] = 'DEMO');

IF @IncubatorId IS NULL
BEGIN
    -- Create a default business incubator
    INSERT INTO [businessincubators].[BusinessIncubators] 
        (ExternalId, Name, Description, [Key], Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        (NEWID(), 'Incubadora Demo', 'Incubadora de demostración para datos de prueba', 'DEMO', 1, GETUTCDATE(), @StarterUserId, 0);
    
    SET @IncubatorId = SCOPE_IDENTITY();
    PRINT '[004.SeedStarterData.sql] Created demo business incubator';
END
ELSE
BEGIN
    PRINT '[004.SeedStarterData.sql] Using existing demo business incubator';
END

-- Create ONE demo project
DECLARE @DemoProjectId BIGINT = (SELECT TOP 1 Id FROM [businessincubators].[Projects] WHERE [Key] = 'INNOV-DEMO');

IF @DemoProjectId IS NULL
BEGIN
    INSERT INTO [businessincubators].[Projects]
        (BusinessIncubatorId, ExternalId, Name, Description, [Key],
         Latitude, Longitude, Geohash, LocationName, LocationAddress,
         Status, CreatedAt, CreatedBy, IsDeleted)
    VALUES
        (@IncubatorId, NEWID(), 'Proyecto Demo Innovación', 'Proyecto demo de innovación tecnológica', 'INNOV-DEMO',
         9.928100, -84.090700, 'd1rgy8bpbpbp', 'San José Centro', 'Avenida Central, San José, Costa Rica',
         1, GETUTCDATE(), @CoordinatorUserId, 0);

    SET @DemoProjectId = SCOPE_IDENTITY();
    PRINT '[004.SeedStarterData.sql] Created demo project with geolocation (San José Centro)';
END
ELSE
BEGIN
    PRINT '[004.SeedStarterData.sql] Using existing demo project';
END

-- Assign the three users to the demo project
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectUsers] WHERE ProjectId = @DemoProjectId AND UserId = @StarterUserId)
BEGIN
    INSERT INTO [businessincubators].[ProjectUsers] (ProjectId, UserId, Role, JoinedAt, IsActive)
    VALUES (@DemoProjectId, @StarterUserId, 'Starter', GETUTCDATE(), 1);
    PRINT '[004.SeedStarterData.sql] Assigned Starter user to demo project';
END

IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectUsers] WHERE ProjectId = @DemoProjectId AND UserId = @MentorUserId)
BEGIN
    INSERT INTO [businessincubators].[ProjectUsers] (ProjectId, UserId, Role, JoinedAt, IsActive)
    VALUES (@DemoProjectId, @MentorUserId, 'Mentor', GETUTCDATE(), 1);
    PRINT '[004.SeedStarterData.sql] Assigned Mentor user to demo project';
END

IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectUsers] WHERE ProjectId = @DemoProjectId AND UserId = @CoordinatorUserId)
BEGIN
    INSERT INTO [businessincubators].[ProjectUsers] (ProjectId, UserId, Role, JoinedAt, IsActive)
    VALUES (@DemoProjectId, @CoordinatorUserId, 'Coordinator', GETUTCDATE(), 1);
    PRINT '[004.SeedStarterData.sql] Assigned Coordinator user to demo project';
END

-- Use the demo project for all subsequent data
SET @Project1Id = @DemoProjectId;
SET @Project2Id = @DemoProjectId;
SET @Project3Id = @DemoProjectId;

-- Verify project was created
IF @DemoProjectId IS NULL
BEGIN
    PRINT '[004.SeedStarterData.sql] Error - Failed to create demo project';
    RETURN;
END

PRINT '[004.SeedStarterData.sql] Using demo project: ' + CAST(@DemoProjectId AS NVARCHAR(10));

-- =============================================
-- Seed StarterProgress Records
-- =============================================
-- Only insert StarterProgress for the Starter user
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[StarterProgress] WHERE UserId = @StarterUserId AND ProjectId = @DemoProjectId)
BEGIN
    INSERT INTO [businessincubators].[StarterProgress]
    (UserId, ProjectId, CurrentPhase, PhaseStartDate, PhaseExpectedEndDate, 
     OverallProgress, PhaseProgress, TasksCompleted, TasksTotal, TasksOverdue,
     FormsCompleted, FormsTotal, FormsRejected, MilestonesAchieved, MilestonesTotal,
     LastActivityDate, NextMilestoneDate, NextMilestoneName, EngagementScore, PerformanceScore)
    VALUES
    -- Starter user in diagnosis phase
    (@StarterUserId, @DemoProjectId, 'diagnosis', DATEADD(day, -7, GETUTCDATE()), DATEADD(day, 23, GETUTCDATE()),
     15.00, 35.00, 3, 12, 1,
     1, 4, 0, 0, 5,
     DATEADD(hour, -2, GETUTCDATE()), DATEADD(day, 7, GETUTCDATE()), 'Completar Diagnóstico Inicial', 75.00, 80.00);
    
    PRINT '[004.SeedStarterData.sql] Created StarterProgress for demo starter user';
END;

-- =============================================
-- Seed StarterTasks Records
-- =============================================

-- Helper to generate tasks for each user/phase
DECLARE @CurrentDate DATETIME2 = GETUTCDATE();

-- Tasks for Starter User (Diagnosis Phase)
-- First, clean up any existing demo tasks to ensure idempotency
DELETE FROM [businessincubators].[StarterTasks]
WHERE UserId = @StarterUserId AND ProjectId = @DemoProjectId;

IF NOT EXISTS (SELECT 1 FROM [businessincubators].[StarterTasks] WHERE UserId = @StarterUserId AND ProjectId = @DemoProjectId)
BEGIN
    INSERT INTO [businessincubators].[StarterTasks]
    (UserId, ProjectId, Title, Description, Type, Category, Priority, Status, Phase,
     EstimatedDuration, DueDate, ActionUrl, ActionText, CreatedAt, CreatedBy)
    VALUES
    -- Pending tasks
    (@StarterUserId, @DemoProjectId, 'Completar Evaluación Inicial', 'Complete el formulario de evaluación diagnóstica de su emprendimiento', 
     'form', 'required', 2, 'pending', 'diagnosis',
     60, DATEADD(day, 3, @CurrentDate), '/ParticipantForm?projectId=1&formType=diagnosis', 'Completar Formulario', @CurrentDate, @StarterUserId),
    
    (@StarterUserId, @DemoProjectId, 'Subir Plan de Negocio', 'Cargue su plan de negocio actualizado en formato PDF', 
     'document', 'required', 1, 'pending', 'diagnosis',
     30, DATEADD(day, 7, @CurrentDate), '/Documents/Upload?type=business-plan', 'Subir Documento', @CurrentDate, @StarterUserId),
    
    (@StarterUserId, @DemoProjectId, 'Agendar Primera Mentoría', 'Programe su sesión inicial con el mentor asignado', 
     'meeting', 'required', 3, 'pending', 'diagnosis',
     15, DATEADD(day, 2, @CurrentDate), '/Meetings/Schedule?type=mentoring', 'Agendar Reunión', @CurrentDate, @StarterUserId),
    
    (@StarterUserId, @DemoProjectId, 'Análisis FODA', 'Complete el análisis de Fortalezas, Oportunidades, Debilidades y Amenazas', 
     'form', 'required', 1, 'pending', 'diagnosis',
     90, DATEADD(day, 10, @CurrentDate), '/ParticipantForm?projectId=1&formType=swot', 'Realizar Análisis', @CurrentDate, @StarterUserId),
    
    (@StarterUserId, @DemoProjectId, 'Definir Modelo Canvas', 'Desarrolle su modelo de negocio usando la metodología Canvas', 
     'document', 'recommended', 1, 'pending', 'diagnosis',
     120, DATEADD(day, 14, @CurrentDate), '/Tools/Canvas', 'Crear Canvas', @CurrentDate, @StarterUserId),
    
    -- In progress tasks
    (@StarterUserId, @DemoProjectId, 'Investigación de Mercado', 'Realice un estudio básico de su mercado objetivo', 
     'custom', 'required', 1, 'in_progress', 'diagnosis',
     240, DATEADD(day, 5, @CurrentDate), NULL, NULL, DATEADD(day, -2, @CurrentDate), @StarterUserId),
    
    -- Completed tasks
    (@StarterUserId, @DemoProjectId, 'Registro en Plataforma', 'Complete su registro y perfil en la plataforma', 
     'form', 'required', 2, 'completed', 'diagnosis',
     15, DATEADD(day, -5, @CurrentDate), '/Profile/Complete', 'Ver Perfil', DATEADD(day, -7, @CurrentDate), @StarterUserId),
    
    (@StarterUserId, @DemoProjectId, 'Asistir a Sesión de Inducción', 'Participe en la sesión de bienvenida del programa', 
     'meeting', 'required', 2, 'completed', 'diagnosis',
     90, DATEADD(day, -3, @CurrentDate), NULL, NULL, DATEADD(day, -6, @CurrentDate), @StarterUserId),
    
    -- Overdue task
    (@StarterUserId, @DemoProjectId, 'Cargar Documentos Legales', 'Suba los documentos de constitución de su empresa', 
     'document', 'optional', 0, 'pending', 'diagnosis',
     20, DATEADD(day, -1, @CurrentDate), '/Documents/Upload?type=legal', 'Subir Documentos', DATEADD(day, -10, @CurrentDate), @StarterUserId);
END;

-- Remove additional user tasks - we only have one starter user in the demo
-- The sections below are commented out as we only seed data for the demo starter user
/*
-- Tasks for User 2 (Development Phase)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[StarterTasks] WHERE UserId = @User2Id)
BEGIN
    INSERT INTO [businessincubators].[StarterTasks]
    (UserId, ProjectId, Title, Description, Type, Category, Priority, Status, Phase,
     EstimatedDuration, DueDate, ActionUrl, ActionText, CreatedAt, CreatedBy)
    VALUES
    -- Development phase tasks
    (@User2Id, @Project1Id, 'Desarrollar MVP', 'Cree la versión mínima viable de su producto', 
     'milestone', 'required', 3, 'in_progress', 'development',
     2880, DATEADD(day, 14, @CurrentDate), '/Projects/MVP', 'Ver Progreso', DATEADD(day, -15, @CurrentDate), @User2Id),
    
    (@User2Id, @Project1Id, 'Crear Plan Financiero', 'Desarrolle proyecciones financieras a 3 años', 
     'document', 'required', 2, 'pending', 'development',
     480, DATEADD(day, 7, @CurrentDate), '/Tools/FinancialPlan', 'Crear Plan', DATEADD(day, -10, @CurrentDate), @User2Id),
    
    (@User2Id, @Project1Id, 'Validar Hipótesis de Mercado', 'Realice entrevistas con clientes potenciales', 
     'form', 'required', 2, 'pending', 'development',
     360, DATEADD(day, 10, @CurrentDate), '/ParticipantForm?projectId=1&formType=market-validation', 'Registrar Validación', DATEADD(day, -8, @CurrentDate), @User2Id),
    
    (@User2Id, @Project1Id, 'Establecer KPIs', 'Defina los indicadores clave de rendimiento', 
     'document', 'required', 1, 'pending', 'development',
     120, DATEADD(day, 5, @CurrentDate), '/Metrics/KPIs', 'Definir KPIs', DATEADD(day, -5, @CurrentDate), @User2Id),
    
    (@User2Id, @Project1Id, 'Sesión de Mentoría Técnica', 'Reunión con mentor especializado en desarrollo', 
     'meeting', 'recommended', 1, 'pending', 'development',
     60, DATEADD(day, 3, @CurrentDate), '/Meetings/Schedule?type=technical', 'Agendar', DATEADD(day, -3, @CurrentDate), @User2Id),
    
    -- Completed development tasks
    (@User2Id, @Project1Id, 'Definir Arquitectura Técnica', 'Diseñe la arquitectura de su solución', 
     'document', 'required', 2, 'completed', 'development',
     240, DATEADD(day, -10, @CurrentDate), NULL, NULL, DATEADD(day, -20, @CurrentDate), @User2Id),
    
    (@User2Id, @Project1Id, 'Crear Mockups', 'Diseñe los prototipos de interfaz de usuario', 
     'custom', 'required', 1, 'completed', 'development',
     360, DATEADD(day, -7, @CurrentDate), NULL, NULL, DATEADD(day, -15, @CurrentDate), @User2Id),
    
    -- Overdue development tasks
    (@User2Id, @Project1Id, 'Registro de Marca', 'Inicie el proceso de registro de su marca', 
     'document', 'optional', 0, 'pending', 'development',
     180, DATEADD(day, -2, @CurrentDate), '/Legal/Trademark', 'Iniciar Registro', DATEADD(day, -12, @CurrentDate), @User2Id),
    
    (@User2Id, @Project1Id, 'Análisis de Competencia', 'Realice un análisis detallado de sus competidores', 
     'form', 'recommended', 1, 'pending', 'development',
     240, DATEADD(day, -1, @CurrentDate), '/ParticipantForm?projectId=1&formType=competition', 'Completar Análisis', DATEADD(day, -10, @CurrentDate), @User2Id);
END;

-- Tasks for User 3 (Validation Phase)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[StarterTasks] WHERE UserId = @User3Id)
BEGIN
    INSERT INTO [businessincubators].[StarterTasks]
    (UserId, ProjectId, Title, Description, Type, Category, Priority, Status, Phase,
     EstimatedDuration, DueDate, ActionUrl, ActionText, CreatedAt, CreatedBy)
    VALUES
    -- Validation phase tasks
    (@User3Id, @Project2Id, 'Pruebas con Usuarios Beta', 'Realice pruebas con un grupo de usuarios beta', 
     'form', 'required', 3, 'in_progress', 'validation',
     720, DATEADD(day, 5, @CurrentDate), '/Testing/BetaUsers', 'Ver Resultados', DATEADD(day, -10, @CurrentDate), @User3Id),
    
    (@User3Id, @Project2Id, 'Ajustar Producto según Feedback', 'Implemente mejoras basadas en retroalimentación', 
     'milestone', 'required', 2, 'pending', 'validation',
     1440, DATEADD(day, 12, @CurrentDate), '/Product/Improvements', 'Ver Mejoras', DATEADD(day, -5, @CurrentDate), @User3Id),
    
    (@User3Id, @Project2Id, 'Preparar Pitch Deck', 'Cree su presentación para inversores', 
     'document', 'required', 2, 'pending', 'validation',
     480, DATEADD(day, 7, @CurrentDate), '/Tools/PitchDeck', 'Crear Pitch', DATEADD(day, -3, @CurrentDate), @User3Id),
    
    (@User3Id, @Project2Id, 'Validar Modelo de Precios', 'Confirme su estrategia de precios con el mercado', 
     'form', 'required', 1, 'pending', 'validation',
     180, DATEADD(day, 4, @CurrentDate), '/ParticipantForm?projectId=2&formType=pricing', 'Validar Precios', @CurrentDate, @User3Id),
    
    -- Completed validation tasks
    (@User3Id, @Project2Id, 'Crear Landing Page', 'Desarrolle una página de aterrizaje para su producto', 
     'custom', 'required', 2, 'completed', 'validation',
     480, DATEADD(day, -14, @CurrentDate), NULL, NULL, DATEADD(day, -20, @CurrentDate), @User3Id),
    
    (@User3Id, @Project2Id, 'Definir Estrategia de Marketing', 'Establezca su plan de marketing digital', 
     'document', 'required', 1, 'completed', 'validation',
     360, DATEADD(day, -7, @CurrentDate), NULL, NULL, DATEADD(day, -15, @CurrentDate), @User3Id),
    
    (@User3Id, @Project2Id, 'Análisis de Métricas', 'Analice las métricas de su producto', 
     'form', 'required', 1, 'completed', 'validation',
     120, DATEADD(day, -3, @CurrentDate), NULL, NULL, DATEADD(day, -10, @CurrentDate), @User3Id);
END;

-- Tasks for User 4 (Implementation Phase)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[StarterTasks] WHERE UserId = @User4Id)
BEGIN
    INSERT INTO [businessincubators].[StarterTasks]
    (UserId, ProjectId, Title, Description, Type, Category, Priority, Status, Phase,
     EstimatedDuration, DueDate, ActionUrl, ActionText, CreatedAt, CreatedBy)
    VALUES
    -- Implementation phase tasks
    (@User4Id, @Project2Id, 'Lanzamiento al Mercado', 'Lance oficialmente su producto al mercado', 
     'milestone', 'required', 3, 'in_progress', 'implementation',
     2880, DATEADD(day, 10, @CurrentDate), '/Launch/Dashboard', 'Ver Lanzamiento', DATEADD(day, -5, @CurrentDate), @User4Id),
    
    (@User4Id, @Project2Id, 'Configurar Analytics', 'Implemente herramientas de análisis y seguimiento', 
     'custom', 'required', 2, 'pending', 'implementation',
     240, DATEADD(day, 3, @CurrentDate), '/Tools/Analytics', 'Configurar', @CurrentDate, @User4Id),
    
    (@User4Id, @Project2Id, 'Establecer Soporte al Cliente', 'Configure sistema de atención al cliente', 
     'custom', 'required', 2, 'pending', 'implementation',
     360, DATEADD(day, 5, @CurrentDate), '/Support/Setup', 'Configurar Soporte', DATEADD(day, -2, @CurrentDate), @User4Id),
    
    (@User4Id, @Project2Id, 'Plan de Ventas Q1', 'Desarrolle su estrategia de ventas para el primer trimestre', 
     'document', 'required', 1, 'pending', 'implementation',
     480, DATEADD(day, 7, @CurrentDate), '/Sales/Plan', 'Crear Plan', DATEADD(day, -1, @CurrentDate), @User4Id),
    
    -- Completed implementation tasks
    (@User4Id, @Project2Id, 'Configurar Pasarela de Pagos', 'Integre sistema de pagos en línea', 
     'custom', 'required', 3, 'completed', 'implementation',
     360, DATEADD(day, -10, @CurrentDate), NULL, NULL, DATEADD(day, -15, @CurrentDate), @User4Id),
    
    (@User4Id, @Project2Id, 'Crear Términos y Condiciones', 'Redacte los términos legales de su servicio', 
     'document', 'required', 1, 'completed', 'implementation',
     240, DATEADD(day, -5, @CurrentDate), NULL, NULL, DATEADD(day, -12, @CurrentDate), @User4Id);
END;

-- Tasks for User 5 (Growth Phase)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[StarterTasks] WHERE UserId = @User5Id)
BEGIN
    INSERT INTO [businessincubators].[StarterTasks]
    (UserId, ProjectId, Title, Description, Type, Category, Priority, Status, Phase,
     EstimatedDuration, DueDate, ActionUrl, ActionText, CreatedAt, CreatedBy)
    VALUES
    -- Growth phase tasks
    (@User5Id, @Project3Id, 'Expansión de Mercado', 'Planifique la expansión a nuevos mercados', 
     'milestone', 'required', 2, 'in_progress', 'growth',
     4320, DATEADD(day, 30, @CurrentDate), '/Expansion/Plan', 'Ver Plan', DATEADD(day, -10, @CurrentDate), @User5Id),
    
    (@User5Id, @Project3Id, 'Búsqueda de Inversión Serie A', 'Prepare documentación para ronda de inversión', 
     'document', 'recommended', 2, 'pending', 'growth',
     960, DATEADD(day, 20, @CurrentDate), '/Investment/SeriesA', 'Preparar Documentos', DATEADD(day, -5, @CurrentDate), @User5Id),
    
    (@User5Id, @Project3Id, 'Optimización de Procesos', 'Mejore la eficiencia operativa', 
     'custom', 'required', 1, 'pending', 'growth',
     720, DATEADD(day, 15, @CurrentDate), '/Operations/Optimize', 'Optimizar', DATEADD(day, -3, @CurrentDate), @User5Id),
    
    -- Completed growth tasks
    (@User5Id, @Project3Id, 'Contratar Equipo Clave', 'Reclute talento esencial para el crecimiento', 
     'custom', 'required', 2, 'completed', 'growth',
     2880, DATEADD(day, -20, @CurrentDate), NULL, NULL, DATEADD(day, -30, @CurrentDate), @User5Id),
    
    (@User5Id, @Project3Id, 'Establecer Partnerships', 'Cree alianzas estratégicas con socios clave', 
     'meeting', 'required', 1, 'completed', 'growth',
     480, DATEADD(day, -10, @CurrentDate), NULL, NULL, DATEADD(day, -25, @CurrentDate), @User5Id);
END;
*/

-- =============================================
-- Update task completion dates where status is completed (DEMO TASKS ONLY)
-- =============================================
UPDATE [businessincubators].[StarterTasks]
SET CompletedAt = DATEADD(hour, -CAST(RAND(CHECKSUM(NEWID())) * 72 AS INT), GETUTCDATE()),
    CompletedBy = UserId,
    ActualDuration = EstimatedDuration + CAST(RAND(CHECKSUM(NEWID())) * 60 AS INT) - 30
WHERE Status = 'completed' 
  AND CompletedAt IS NULL
  AND UserId = @StarterUserId  -- Only update demo user's tasks
  AND ProjectId = @DemoProjectId;  -- Only update demo project tasks

-- Update started dates for in_progress tasks (DEMO TASKS ONLY)
UPDATE [businessincubators].[StarterTasks]
SET StartedAt = DATEADD(day, -CAST(RAND(CHECKSUM(NEWID())) * 5 AS INT), GETUTCDATE())
WHERE Status = 'in_progress' 
  AND StartedAt IS NULL
  AND UserId = @StarterUserId  -- Only update demo user's tasks
  AND ProjectId = @DemoProjectId;  -- Only update demo project tasks

-- =============================================
-- Create sample notifications for demo users
-- =============================================
-- Clean up old demo notifications first to ensure fresh data
DELETE FROM [core].[UserNotifications]
WHERE UserId IN (@StarterUserId, @MentorUserId, @CoordinatorUserId)
  AND ActionUrl LIKE '%/StarterDashboard/Task/%'
  OR ActionUrl LIKE '%/MentorDashboard%'
  OR ActionUrl LIKE '%/CoordinatorDashboard%';

IF NOT EXISTS (SELECT 1 FROM [core].[UserNotifications] WHERE UserId = @StarterUserId AND ActionUrl LIKE '%/StarterDashboard/Task/%')
BEGIN
    INSERT INTO [core].[UserNotifications]
    (UserId, Title, Message, Type, Category, Priority, ActionUrl, ActionText, IsRead, CreatedAt)
    VALUES
    (@StarterUserId, 'Tarea Próxima a Vencer', 'La tarea "Completar Evaluación Inicial" vence en 3 días', 
     'Task', 'Reminder', 1, '/StarterDashboard/Task/1', 'Ver Tarea', 0, DATEADD(hour, -1, GETUTCDATE())),
    
    (@StarterUserId, 'Nueva Mentoría Disponible', 'Su mentor ha abierto nuevos horarios para sesiones', 
     'Message', 'Info', 0, '/Meetings/Available', 'Ver Horarios', 0, DATEADD(hour, -12, GETUTCDATE())),
    
    (@StarterUserId, 'Recurso Recomendado', 'Nuevo material disponible: Guía de Plan de Negocios', 
     'System', 'Resource', 0, '/Resources/Guide/business-plan', 'Ver Guía', 1, DATEADD(day, -1, GETUTCDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM [core].[UserNotifications] WHERE UserId = @MentorUserId)
BEGIN
    INSERT INTO [core].[UserNotifications]
    (UserId, Title, Message, Type, Category, Priority, ActionUrl, ActionText, IsRead, CreatedAt)
    VALUES
    (@MentorUserId, 'Nueva Asignación', 'Se te ha asignado un nuevo emprendedor para mentoría', 
     'Message', 'Info', 1, '/MentorDashboard', 'Ver Dashboard', 0, DATEADD(hour, -6, GETUTCDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM [core].[UserNotifications] WHERE UserId = @CoordinatorUserId)
BEGIN
    INSERT INTO [core].[UserNotifications]
    (UserId, Title, Message, Type, Category, Priority, ActionUrl, ActionText, IsRead, CreatedAt)
    VALUES
    (@CoordinatorUserId, 'Proyecto Creado', 'El proyecto demo ha sido creado exitosamente', 
     'System', 'Info', 0, '/CoordinatorDashboard', 'Ver Dashboard', 0, DATEADD(hour, -4, GETUTCDATE()));
END;

-- =============================================
-- Seed StarterResources Records
-- =============================================
-- Clean up old demo resources first
DELETE FROM [businessincubators].[StarterResources]
WHERE ProjectId = @DemoProjectId;

IF NOT EXISTS (SELECT 1 FROM [businessincubators].[StarterResources] WHERE ProjectId = @DemoProjectId)
BEGIN
    INSERT INTO [businessincubators].[StarterResources]
    (ProjectId, Category, Title, Description, ResourceType, Url, Phase, [Order], IsRequired, CreatedBy)
    VALUES
    -- Diagnosis Phase Resources
    (@DemoProjectId, 'guide', 'Guía de Diagnóstico Empresarial', 'Guía completa para realizar el diagnóstico inicial de tu emprendimiento', 'pdf', '/resources/guides/diagnostico-empresarial.pdf', 'diagnosis', 1, 1, 'system'),
    (@DemoProjectId, 'template', 'Plantilla Canvas', 'Modelo Canvas para definir tu modelo de negocio', 'template', '/resources/templates/canvas.xlsx', 'diagnosis', 2, 1, 'system'),
    (@DemoProjectId, 'video', 'Cómo hacer un FODA', 'Video tutorial sobre análisis FODA', 'youtube', 'https://youtube.com/watch?v=example1', 'diagnosis', 3, 0, 'system'),
    (@DemoProjectId, 'article', 'Investigación de Mercado', 'Artículo sobre técnicas de investigación de mercado', 'link', 'https://blog.incubadora.com/investigacion-mercado', 'diagnosis', 4, 0, 'system'),
    
    -- Development Phase Resources  
    (@DemoProjectId, 'guide', 'Desarrollo de MVP', 'Guía para crear tu producto mínimo viable', 'pdf', '/resources/guides/mvp-guide.pdf', 'development', 1, 1, 'system'),
    (@DemoProjectId, 'tool', 'Calculadora Financiera', 'Herramienta para proyecciones financieras', 'link', 'https://tools.incubadora.com/calculadora', 'development', 2, 1, 'system'),
    (@DemoProjectId, 'template', 'Plan de Negocios', 'Plantilla de plan de negocios', 'doc', '/resources/templates/business-plan.docx', 'development', 3, 1, 'system'),
    (@DemoProjectId, 'course', 'Curso de Finanzas', 'Curso online de finanzas para emprendedores', 'link', 'https://cursos.incubadora.com/finanzas', 'development', 4, 0, 'system'),
    
    -- Validation Phase Resources
    (@DemoProjectId, 'guide', 'Validación con Usuarios', 'Metodología para validar tu producto', 'pdf', '/resources/guides/validacion.pdf', 'validation', 1, 1, 'system'),
    (@DemoProjectId, 'template', 'Encuesta de Validación', 'Plantilla de encuesta para usuarios beta', 'template', '/resources/templates/survey.docx', 'validation', 2, 0, 'system'),
    (@DemoProjectId, 'example', 'Casos de Éxito', 'Ejemplos de validaciones exitosas', 'pdf', '/resources/examples/casos-exito.pdf', 'validation', 3, 0, 'system'),
    
    -- Implementation Phase Resources
    (@DemoProjectId, 'guide', 'Lanzamiento al Mercado', 'Estrategia de go-to-market', 'pdf', '/resources/guides/go-to-market.pdf', 'implementation', 1, 1, 'system'),
    (@DemoProjectId, 'tool', 'Analytics Dashboard', 'Panel de métricas y KPIs', 'link', 'https://analytics.incubadora.com', 'implementation', 2, 1, 'system'),
    (@DemoProjectId, 'template', 'Plan de Marketing', 'Plantilla de plan de marketing digital', 'ppt', '/resources/templates/marketing-plan.pptx', 'implementation', 3, 0, 'system'),
    
    -- Growth Phase Resources
    (@DemoProjectId, 'guide', 'Estrategia de Crecimiento', 'Guía para escalar tu negocio', 'pdf', '/resources/guides/growth-strategy.pdf', 'growth', 1, 0, 'system'),
    (@DemoProjectId, 'document', 'Preparación para Inversión', 'Documentación para rondas de inversión', 'pdf', '/resources/docs/investment-prep.pdf', 'growth', 2, 0, 'system'),
    (@DemoProjectId, 'course', 'Masterclass Escalamiento', 'Curso avanzado de escalamiento empresarial', 'video', 'https://videos.incubadora.com/masterclass', 'growth', 3, 0, 'system');
END;

-- =============================================
-- Seed ProjectMentorAssignments
-- =============================================
-- Use MERGE to safely manage demo mentor assignment
MERGE [businessincubators].[ProjectMentorAssignments] AS target
USING (SELECT 
    @MentorUserId AS MentorUserId,
    @DemoProjectId AS ProjectId,
    @StarterUserId AS StarterUserId,
    DATEADD(day, -30, GETUTCDATE()) AS AssignedDate,
    'active' AS Status,
    12 AS TotalSessions,
    2 AS CompletedSessions,
    DATEADD(day, 7, GETUTCDATE()) AS NextSessionDate,
    'Business Strategy, Marketing, Finance' AS MentorSpecialties,
    'virtual' AS PreferredMeetingType,
    @CoordinatorUserId AS CreatedBy
) AS source
ON target.ProjectId = source.ProjectId 
   AND target.StarterUserId = source.StarterUserId 
   AND target.MentorUserId = source.MentorUserId
WHEN MATCHED THEN
    UPDATE SET 
        AssignedDate = source.AssignedDate,
        Status = source.Status,
        TotalSessions = source.TotalSessions,
        CompletedSessions = source.CompletedSessions,
        NextSessionDate = source.NextSessionDate,
        MentorSpecialties = source.MentorSpecialties,
        PreferredMeetingType = source.PreferredMeetingType
WHEN NOT MATCHED THEN
    INSERT (MentorUserId, ProjectId, StarterUserId, AssignedDate, Status, TotalSessions, 
            CompletedSessions, NextSessionDate, MentorSpecialties, PreferredMeetingType, CreatedBy)
    VALUES (source.MentorUserId, source.ProjectId, source.StarterUserId, source.AssignedDate, 
            source.Status, source.TotalSessions, source.CompletedSessions, source.NextSessionDate, 
            source.MentorSpecialties, source.PreferredMeetingType, source.CreatedBy);

IF @@ROWCOUNT > 0
BEGIN
    PRINT '[006.SeedStarterData.sql] Created or updated mentor assignment for demo project';
END
ELSE
BEGIN
    PRINT '[006.SeedStarterData.sql] Mentor assignment already exists for demo project';
END

PRINT '[006.SeedStarterData.sql] Finished (only demo data affected)';
PRINT 'Starter seed data completed successfully';

GO