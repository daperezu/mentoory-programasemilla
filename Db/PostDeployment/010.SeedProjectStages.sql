-- ==========================================================================
-- Seed Data: Default Project Stages
-- Purpose: Add default stages for existing projects
-- Created: 2025-08-22
-- ==========================================================================

PRINT 'Seeding default project stages...';

-- Add default stages for all existing projects that don't have stages yet
DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @SystemUser NVARCHAR(100) = 'SYSTEM';

-- Insert default stages for projects without stages
INSERT INTO [businessincubators].[ProjectStages] (
    [ProjectId],
    [Type],
    [Title],
    [Description],
    [StartDate],
    [EndDate],
    [IsActive],
    [CreatedAt],
    [CreatedBy],
    [UpdatedAt],
    [UpdatedBy]
)
SELECT 
    p.Id,
    stageType.Type,
    stageType.Title,
    stageType.Description,
    CASE stageType.Type
        WHEN 1 THEN @CurrentDate -- Invitation starts now
        WHEN 2 THEN DATEADD(DAY, 30, @CurrentDate) -- InitialFormCollection starts in 30 days
        WHEN 3 THEN DATEADD(DAY, 60, @CurrentDate) -- Mentoring starts in 60 days
        WHEN 4 THEN DATEADD(DAY, 180, @CurrentDate) -- FinalFormCollection starts in 180 days
        WHEN 5 THEN DATEADD(DAY, 210, @CurrentDate) -- Closure starts in 210 days
    END AS StartDate,
    CASE stageType.Type
        WHEN 1 THEN DATEADD(DAY, 30, @CurrentDate) -- Invitation ends in 30 days
        WHEN 2 THEN DATEADD(DAY, 60, @CurrentDate) -- InitialFormCollection ends in 60 days
        WHEN 3 THEN DATEADD(DAY, 180, @CurrentDate) -- Mentoring ends in 180 days
        WHEN 4 THEN DATEADD(DAY, 210, @CurrentDate) -- FinalFormCollection ends in 210 days
        WHEN 5 THEN DATEADD(DAY, 240, @CurrentDate) -- Closure ends in 240 days
    END AS EndDate,
    CASE stageType.Type
        WHEN 1 THEN 1 -- Invitation stage is active by default
        ELSE 0
    END AS IsActive,
    @CurrentDate,
    @SystemUser,
    NULL,
    NULL
FROM [businessincubators].[Projects] p
CROSS JOIN (
    VALUES 
        (1, N'Invitación', N'Periodo de invitación y registro de participantes'),
        (2, N'Formularios Iniciales', N'Recolección de información inicial de los emprendedores'),
        (3, N'Mentoría', N'Periodo de acompañamiento y desarrollo del emprendimiento'),
        (4, N'Formularios Finales', N'Evaluación final y recolección de resultados'),
        (5, N'Cierre', N'Cierre del proyecto y evaluación de impacto')
) AS stageType(Type, Title, Description)
WHERE NOT EXISTS (
    SELECT 1 
    FROM [businessincubators].[ProjectStages] ps 
    WHERE ps.ProjectId = p.Id AND ps.Type = stageType.Type
)
AND p.IsDeleted = 0;

-- Count and display results
DECLARE @InsertedCount INT = @@ROWCOUNT;
PRINT 'Inserted ' + CAST(@InsertedCount AS NVARCHAR(10)) + ' project stages.';

-- Verify the insertion
DECLARE @ProjectCount INT = (SELECT COUNT(DISTINCT ProjectId) FROM [businessincubators].[ProjectStages]);
DECLARE @TotalStages INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectStages]);
PRINT 'Total projects with stages: ' + CAST(@ProjectCount AS NVARCHAR(10));
PRINT 'Total stages created: ' + CAST(@TotalStages AS NVARCHAR(10));

GO