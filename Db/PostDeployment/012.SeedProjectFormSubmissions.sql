-- =============================================
-- Seed Data for Project Form Submissions
-- Created: 2025-09-07
-- Description: Creates initial form submissions for demo project starter user
--              and activates the InitialFormCollection stage
-- IMPORTANT: This script is idempotent and safe to run multiple times
-- =============================================

-- =============================================
-- Get Demo Project and User IDs
-- =============================================
DECLARE @DemoProjectId BIGINT = (SELECT TOP 1 Id FROM [businessincubators].[Projects] WHERE [Key] = 'INNOV-DEMO');
DECLARE @DemoProjectExternalId UNIQUEIDENTIFIER = (SELECT TOP 1 ExternalId FROM [businessincubators].[Projects] WHERE [Key] = 'INNOV-DEMO');
DECLARE @StarterUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'demo.starter');

-- Verify project and user exist
IF @DemoProjectId IS NULL OR @StarterUserId IS NULL
BEGIN
    PRINT '[012.SeedProjectFormSubmissions.sql] Error - Demo project or starter user not found.';
    RETURN;
END

PRINT '[012.SeedProjectFormSubmissions.sql] Using demo project ID: ' + CAST(@DemoProjectId AS NVARCHAR(10));
PRINT '[012.SeedProjectFormSubmissions.sql] Using starter user: demo.starter';

-- =============================================
-- Update Project Stages for Demo Project
-- =============================================
-- First, update the InitialFormCollection stage to be active and within current dates
DECLARE @CurrentDate DATETIME2 = GETUTCDATE();

-- Update InitialFormCollection stage (Type = 2) to be active now
UPDATE [businessincubators].[ProjectStages]
SET 
    StartDate = DATEADD(DAY, -5, @CurrentDate), -- Started 5 days ago
    EndDate = DATEADD(DAY, 25, @CurrentDate),   -- Ends in 25 days (30 day window)
    IsActive = 1,
    UpdatedAt = @CurrentDate,
    UpdatedBy = 'SEED_SCRIPT'
WHERE ProjectId = @DemoProjectId 
    AND [Type] = 2; -- InitialFormCollection

IF @@ROWCOUNT > 0
BEGIN
    PRINT '[012.SeedProjectFormSubmissions.sql] Updated InitialFormCollection stage to be active';
END

-- Deactivate other stages for clarity
UPDATE [businessincubators].[ProjectStages]
SET 
    IsActive = 0,
    UpdatedAt = @CurrentDate,
    UpdatedBy = 'SEED_SCRIPT'
WHERE ProjectId = @DemoProjectId 
    AND [Type] != 2; -- Not InitialFormCollection

-- =============================================
-- Get Knowledge Structure ID
-- =============================================
DECLARE @KnowledgeStructureId BIGINT = (
    SELECT TOP 1 Id 
    FROM [businessincubators].[ProjectKnowledgeStructures] 
    WHERE ProjectId = @DemoProjectId
);

IF @KnowledgeStructureId IS NULL
BEGIN
    PRINT '[012.SeedProjectFormSubmissions.sql] Error - Knowledge structure not found for demo project.';
    PRINT '[012.SeedProjectFormSubmissions.sql] Please run 011.SeedProjectKnowledgeStructure.sql first.';
    RETURN;
END

-- =============================================
-- Get Stage ID for InitialFormCollection
-- =============================================
DECLARE @StageId BIGINT = (
    SELECT TOP 1 Id 
    FROM [businessincubators].[ProjectStages] 
    WHERE ProjectId = @DemoProjectId 
    AND [Type] = 2 -- InitialFormCollection
);

IF @StageId IS NULL
BEGIN
    PRINT '[012.SeedProjectFormSubmissions.sql] Error - InitialFormCollection stage not found.';
    RETURN;
END

-- =============================================
-- Create Form Submission for Start Phase
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM [businessincubators].[ProjectFormSubmissions] 
    WHERE ProjectId = @DemoProjectId 
    AND ParticipantUserId = @StarterUserId
    AND Phase = 1 -- Start phase (1=Start, 2=Final, 4=Undefined)
)
BEGIN
    INSERT INTO [businessincubators].[ProjectFormSubmissions] (
        ExternalId,
        ProjectId,
        ParticipantUserId,
        Phase,
        Status,
        StartedAt,
        ProjectStageId,
        TotalQuestions,
        AnsweredQuestions,
        CompletionPercentage,
        FormSchemaVersion
    )
    VALUES (
        NEWID(),
        @DemoProjectId,
        @StarterUserId,
        1, -- Start phase
        1, -- Draft status
        @CurrentDate,
        @StageId,
        0, -- Will be calculated when form is loaded
        0, -- No questions answered yet
        0, -- 0% complete
        1  -- Schema version 1
    );
    
    PRINT '[012.SeedProjectFormSubmissions.sql] Created form submission for demo.starter';
END
ELSE
BEGIN
    PRINT '[012.SeedProjectFormSubmissions.sql] Form submission already exists for demo.starter';
END

-- =============================================
-- Verification
-- =============================================
PRINT '';
PRINT '=== VERIFICATION RESULTS ===';

-- Verify active stage
DECLARE @ActiveStageCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectStages] 
    WHERE ProjectId = @DemoProjectId 
    AND IsActive = 1
    AND [Type] = 2
);
PRINT '✓ Active InitialFormCollection stages: ' + CAST(@ActiveStageCount AS NVARCHAR(10));

-- Verify stage dates
DECLARE @StageStartDate DATETIME2, @StageEndDate DATETIME2;
SELECT 
    @StageStartDate = StartDate,
    @StageEndDate = EndDate
FROM [businessincubators].[ProjectStages] 
WHERE ProjectId = @DemoProjectId 
AND [Type] = 2;

PRINT '✓ Stage dates: ' + CONVERT(NVARCHAR(30), @StageStartDate, 120) + ' to ' + CONVERT(NVARCHAR(30), @StageEndDate, 120);

-- Check if current date is within stage dates
IF @CurrentDate BETWEEN @StageStartDate AND @StageEndDate
BEGIN
    PRINT '✓ Current date is within stage window';
END
ELSE
BEGIN
    PRINT '✗ WARNING: Current date is NOT within stage window';
END

-- Verify form submission
DECLARE @FormCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectFormSubmissions] 
    WHERE ProjectId = @DemoProjectId 
    AND ParticipantUserId = @StarterUserId
);
PRINT '✓ Form submissions created: ' + CAST(@FormCount AS NVARCHAR(10));

PRINT '';
PRINT '[012.SeedProjectFormSubmissions.sql] Completed successfully!';
PRINT 'demo.starter user should now see a pending form on the Participant Dashboard.';

GO