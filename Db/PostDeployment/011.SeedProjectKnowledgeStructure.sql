-- =============================================
-- Seed Data for Project Knowledge Structure
-- Created: 2025-09-06
-- Updated: 2025-09-08
-- Description: Creates complete hierarchy: ProjectKnowledgeStructure → ProjectModules → ProjectTopics → ProjectQuestions
--              for the demo project to enable form functionality for starter users
-- IMPORTANT: This script is idempotent and safe to run multiple times
-- NOTE: This seeds data for the 'INNOV-DEMO' project. To test, use the project with [Key] = 'INNOV-DEMO'
--       The project's ExternalId is generated dynamically with NEWID() in 006.SeedStarterData.sql
-- =============================================

-- =============================================
-- Get Demo Project ID (created in 006.SeedStarterData.sql)
-- =============================================
DECLARE @DemoProjectId BIGINT = (SELECT TOP 1 Id FROM [businessincubators].[Projects] WHERE [Key] = 'INNOV-DEMO');

-- Verify project exists
IF @DemoProjectId IS NULL
BEGIN
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Error - Demo project not found. Please ensure 006.SeedStarterData.sql has been executed.';
    RETURN;
END

PRINT '[011.SeedProjectKnowledgeStructure.sql] Using demo project ID: ' + CAST(@DemoProjectId AS NVARCHAR(10));

-- =============================================
-- Create ProjectKnowledgeStructure
-- =============================================
DECLARE @KnowledgeStructureId BIGINT;

-- Check if structure already exists
SELECT @KnowledgeStructureId = Id FROM [businessincubators].[ProjectKnowledgeStructures] 
WHERE ProjectId = @DemoProjectId;

IF @KnowledgeStructureId IS NULL
BEGIN
    INSERT INTO [businessincubators].[ProjectKnowledgeStructures]
        (ProjectId, Name, Description, CurrentVersion, IsLocked, IsNameCustomized, IsDescriptionCustomized)
    VALUES
        (@DemoProjectId, 
         'Estructura de Conocimiento - Innovación Tecnológica', 
         'Estructura completa para evaluar proyectos de innovación tecnológica en todas sus fases',
         1,
         0,
         0,
         0);
    
    SET @KnowledgeStructureId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created ProjectKnowledgeStructure with ID: ' + CAST(@KnowledgeStructureId AS NVARCHAR(10));
END
ELSE
BEGIN
    PRINT '[011.SeedProjectKnowledgeStructure.sql] ProjectKnowledgeStructure already exists with ID: ' + CAST(@KnowledgeStructureId AS NVARCHAR(10));
END

-- =============================================
-- Create ProjectModules (Required for hierarchy)
-- =============================================
DECLARE @ModuleGeneralId BIGINT;
DECLARE @ModuleNegocioId BIGINT;
DECLARE @ModuleMercadoId BIGINT;
DECLARE @ModuleFinancieroId BIGINT;
DECLARE @ModuleInnovacionId BIGINT;

-- Module 1: Módulo General
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectModules] WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo General')
BEGIN
    INSERT INTO [businessincubators].[ProjectModules]
        (ProjectKnowledgeStructureId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@KnowledgeStructureId, 
         'Módulo General',
         0,
         1,
         0);
    
    SET @ModuleGeneralId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Module: Módulo General';
END
ELSE
BEGIN
    SELECT @ModuleGeneralId = Id FROM [businessincubators].[ProjectModules] 
    WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo General';
END

-- Module 2: Módulo de Negocio
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectModules] WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo de Negocio')
BEGIN
    INSERT INTO [businessincubators].[ProjectModules]
        (ProjectKnowledgeStructureId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@KnowledgeStructureId, 
         'Módulo de Negocio',
         0,
         2,
         0);
    
    SET @ModuleNegocioId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Module: Módulo de Negocio';
END
ELSE
BEGIN
    SELECT @ModuleNegocioId = Id FROM [businessincubators].[ProjectModules] 
    WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo de Negocio';
END

-- Module 3: Módulo de Mercado
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectModules] WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo de Mercado')
BEGIN
    INSERT INTO [businessincubators].[ProjectModules]
        (ProjectKnowledgeStructureId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@KnowledgeStructureId, 
         'Módulo de Mercado',
         0,
         3,
         0);
    
    SET @ModuleMercadoId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Module: Módulo de Mercado';
END
ELSE
BEGIN
    SELECT @ModuleMercadoId = Id FROM [businessincubators].[ProjectModules] 
    WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo de Mercado';
END

-- Module 4: Módulo Financiero
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectModules] WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo Financiero')
BEGIN
    INSERT INTO [businessincubators].[ProjectModules]
        (ProjectKnowledgeStructureId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@KnowledgeStructureId, 
         'Módulo Financiero',
         0,
         4,
         0);
    
    SET @ModuleFinancieroId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Module: Módulo Financiero';
END
ELSE
BEGIN
    SELECT @ModuleFinancieroId = Id FROM [businessincubators].[ProjectModules] 
    WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo Financiero';
END

-- Module 5: Módulo de Innovación y Tecnología
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectModules] WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo de Innovación y Tecnología')
BEGIN
    INSERT INTO [businessincubators].[ProjectModules]
        (ProjectKnowledgeStructureId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@KnowledgeStructureId, 
         'Módulo de Innovación y Tecnología',
         0,
         5,
         0);
    
    SET @ModuleInnovacionId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Module: Módulo de Innovación y Tecnología';
END
ELSE
BEGIN
    SELECT @ModuleInnovacionId = Id FROM [businessincubators].[ProjectModules] 
    WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo de Innovación y Tecnología';
END

-- =============================================
-- Create ProjectTopics (Required for hierarchy)
-- =============================================
DECLARE @TopicInfoGeneralId BIGINT;
DECLARE @TopicPlanNegocioId BIGINT;
DECLARE @TopicAnalisisMercadoId BIGINT;
DECLARE @TopicFinanzasId BIGINT;
DECLARE @TopicInnovacionId BIGINT;

-- Topic 1: Información General (under Module 1)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleGeneralId AND Name = 'Información General')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleGeneralId, 
         'Información General',
         0,
         1,
         0);
    
    SET @TopicInfoGeneralId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Información General';
END
ELSE
BEGIN
    SELECT @TopicInfoGeneralId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleGeneralId AND Name = 'Información General';
END

-- Topic 2: Plan de Negocio (under Module 2)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleNegocioId AND Name = 'Plan de Negocio')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleNegocioId, 
         'Plan de Negocio',
         0,
         1,
         0);
    
    SET @TopicPlanNegocioId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Plan de Negocio';
END
ELSE
BEGIN
    SELECT @TopicPlanNegocioId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleNegocioId AND Name = 'Plan de Negocio';
END

-- Topic 3: Análisis de Mercado (under Module 3)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleMercadoId AND Name = 'Análisis de Mercado')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleMercadoId, 
         'Análisis de Mercado',
         0,
         1,
         0);
    
    SET @TopicAnalisisMercadoId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Análisis de Mercado';
END
ELSE
BEGIN
    SELECT @TopicAnalisisMercadoId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleMercadoId AND Name = 'Análisis de Mercado';
END

-- Topic 4: Finanzas y Proyecciones (under Module 4)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleFinancieroId AND Name = 'Finanzas y Proyecciones')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleFinancieroId, 
         'Finanzas y Proyecciones',
         0,
         1,
         0);
    
    SET @TopicFinanzasId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Finanzas y Proyecciones';
END
ELSE
BEGIN
    SELECT @TopicFinanzasId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleFinancieroId AND Name = 'Finanzas y Proyecciones';
END

-- Topic 5: Innovación Tecnológica (under Module 5)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleInnovacionId AND Name = 'Innovación Tecnológica')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleInnovacionId, 
         'Innovación Tecnológica',
         0,
         1,
         0);
    
    SET @TopicInnovacionId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Innovación Tecnológica';
END
ELSE
BEGIN
    SELECT @TopicInnovacionId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleInnovacionId AND Name = 'Innovación Tecnológica';
END

-- =============================================
-- Create ProjectBlocks (5 blocks total)
-- =============================================
DECLARE @BlockInfoGeneralId BIGINT;
DECLARE @BlockPlanNegocioId BIGINT;
DECLARE @BlockAnalisisMercadoId BIGINT;
DECLARE @BlockFinancieroId BIGINT;
DECLARE @BlockInnovacionId BIGINT;

-- Block 1: Información General (15 questions)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId AND Name = 'Información General')
BEGIN
    INSERT INTO [businessincubators].[ProjectBlocks]
        (ProjectId, Name, IsNameCustomized)
    VALUES
        (@DemoProjectId, 
         'Información General',
         0);
    
    SET @BlockInfoGeneralId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Block: Información General';
END
ELSE
BEGIN
    SELECT @BlockInfoGeneralId = Id FROM [businessincubators].[ProjectBlocks] 
    WHERE ProjectId = @DemoProjectId AND Name = 'Información General';
END

-- Block 2: Plan de Negocio
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId AND Name = 'Plan de Negocio')
BEGIN
    INSERT INTO [businessincubators].[ProjectBlocks]
        (ProjectId, Name, IsNameCustomized)
    VALUES
        (@DemoProjectId, 
         'Plan de Negocio',
         0);
    
    SET @BlockPlanNegocioId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Block: Plan de Negocio';
END
ELSE
BEGIN
    SELECT @BlockPlanNegocioId = Id FROM [businessincubators].[ProjectBlocks] 
    WHERE ProjectId = @DemoProjectId AND Name = 'Plan de Negocio';
END

-- Block 3: Análisis de Mercado
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId AND Name = 'Análisis de Mercado')
BEGIN
    INSERT INTO [businessincubators].[ProjectBlocks]
        (ProjectId, Name, IsNameCustomized)
    VALUES
        (@DemoProjectId, 
         'Análisis de Mercado',
         0);
    
    SET @BlockAnalisisMercadoId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Block: Análisis de Mercado';
END
ELSE
BEGIN
    SELECT @BlockAnalisisMercadoId = Id FROM [businessincubators].[ProjectBlocks] 
    WHERE ProjectId = @DemoProjectId AND Name = 'Análisis de Mercado';
END

-- Block 4: Análisis Financiero
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId AND Name = 'Análisis Financiero')
BEGIN
    INSERT INTO [businessincubators].[ProjectBlocks]
        (ProjectId, Name, IsNameCustomized)
    VALUES
        (@DemoProjectId, 
         'Análisis Financiero',
         0);
    
    SET @BlockFinancieroId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Block: Análisis Financiero';
END
ELSE
BEGIN
    SELECT @BlockFinancieroId = Id FROM [businessincubators].[ProjectBlocks] 
    WHERE ProjectId = @DemoProjectId AND Name = 'Análisis Financiero';
END

-- Block 5: Innovación y Tecnología
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId AND Name = 'Innovación y Tecnología')
BEGIN
    INSERT INTO [businessincubators].[ProjectBlocks]
        (ProjectId, Name, IsNameCustomized)
    VALUES
        (@DemoProjectId, 
         'Innovación y Tecnología',
         0);
    
    SET @BlockInnovacionId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Block: Innovación y Tecnología';
END
ELSE
BEGIN
    SELECT @BlockInnovacionId = Id FROM [businessincubators].[ProjectBlocks] 
    WHERE ProjectId = @DemoProjectId AND Name = 'Innovación y Tecnología';
END

-- =============================================
-- Create ProjectQuestions for Block 1: Información General (15 questions - including specialized types)
-- =============================================
DECLARE @Q1_1Id BIGINT, @Q1_2Id BIGINT, @Q1_3Id BIGINT, @Q1_4Id BIGINT, @Q1_5Id BIGINT, @Q1_6Id BIGINT, @Q1_7Id BIGINT, @Q1_8Id BIGINT;

-- Question 1.1: Nombre del emprendimiento (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan, 
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         '¿Cuál es el nombre de su emprendimiento?',
         'Ingrese el nombre comercial o marca de su proyecto',
         3, -- FreeText
         1,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_1Id = SCOPE_IDENTITY();
END

-- Question 1.2: Fecha de fundación (Date)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         '¿Cuándo fue fundado su emprendimiento?',
         'Seleccione la fecha de inicio de su proyecto',
         5, -- Date
         2,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_2Id = SCOPE_IDENTITY();
END

-- Question 1.3: Etapa del emprendimiento (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         '¿En qué etapa se encuentra su emprendimiento?',
         'Seleccione la etapa que mejor describe el estado actual de su proyecto',
         1, -- SingleChoice
         3,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_3Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q1_3Id, 'Idea', 1, 'D', 'Etapa muy temprana', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_3Id, 'Prototipo', 2, 'F', 'Desarrollo inicial', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_3Id, 'MVP (Producto Mínimo Viable)', 3, 'F', 'Producto básico funcional', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_3Id, 'Producto en el mercado', 4, 'F', 'Validación de mercado', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_3Id, 'Escalamiento', 5, 'F', 'Crecimiento sostenido', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 1.4: Número de empleados (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 4)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         '¿Cuántos empleados tiene actualmente su empresa?',
         'Incluya empleados de tiempo completo y medio tiempo',
         4, -- Numeric
         4,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_4Id = SCOPE_IDENTITY();
END

-- Question 1.5: Principales desafíos (MultiChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 5)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         '¿Cuáles son los principales desafíos que enfrenta actualmente?',
         'Puede seleccionar múltiples opciones',
         2, -- MultiChoice
         5,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_5Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q1_5Id, 'Financiamiento', 1, 'D', 'Falta de recursos', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Desarrollo de producto', 1, 'D', 'Capacidad técnica limitada', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Marketing y ventas', 1, 'D', 'Alcance de mercado limitado', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Gestión del equipo', 1, 'D', 'Desafíos organizacionales', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Aspectos legales', 1, 'A', 'Riesgo regulatorio', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Tecnología', 1, 'D', 'Infraestructura técnica', 'N', 'Sin impacto ODS', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Competencia', 1, 'A', 'Mercado competitivo', 'N', 'Sin impacto ODS', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Logística', 1, 'D', 'Cadena de suministro', 'N', 'Sin impacto ODS', 8, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 1.6: Descripción del producto/servicio (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 6)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         'Describa brevemente su producto o servicio principal',
         'Máximo 500 caracteres',
         3, -- FreeText
         6,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_6Id = SCOPE_IDENTITY();
END

-- Question 1.7: Sector de la industria (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 7)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         '¿En qué sector de la industria opera su emprendimiento?',
         'Seleccione el sector principal',
         1, -- SingleChoice
         7,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_7Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q1_7Id, 'Tecnología', 4, 'O', 'Sector de alto crecimiento', 'S', 'Innovación tecnológica', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_7Id, 'Salud', 4, 'O', 'Alta demanda', 'S', 'Salud y bienestar', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_7Id, 'Educación', 3, 'O', 'Impacto social', 'S', 'Educación de calidad', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_7Id, 'Finanzas', 4, 'F', 'Sector lucrativo', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_7Id, 'Comercio', 3, 'F', 'Mercado establecido', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_7Id, 'Manufactura', 3, 'F', 'Producción local', 'S', 'Industria e innovación', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_7Id, 'Agricultura', 3, 'O', 'Sector primario', 'S', 'Hambre cero', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_7Id, 'Servicios', 3, 'F', 'Amplio mercado', 'N', 'Sin impacto ODS', 8, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_7Id, 'Otro', 2, 'D', 'Sector no definido', 'N', 'Sin impacto ODS', 9, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 1.8: Años de experiencia del fundador (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 8)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         '¿Cuántos años de experiencia tiene el fundador principal en el sector?',
         'Ingrese el número de años',
         4, -- Numeric
         8,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_8Id = SCOPE_IDENTITY();
END

-- Question 1.9: Identificación del fundador (PersonId)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 9)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         'Número de identificación del fundador principal',
         'Ingrese su número de documento',
         6, -- PersonId
         9,
         1,
         0,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    DECLARE @Q1_9Id BIGINT = SCOPE_IDENTITY();
END

-- Question 1.10: Tipo de identificación (IdType)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 10)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         'Tipo de documento de identificación',
         'Seleccione el tipo de documento',
         7, -- IdType
         10,
         1,
         0,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    DECLARE @Q1_10Id BIGINT = SCOPE_IDENTITY();
END

-- Question 1.11: Género del fundador (Gender)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 11)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         'Género del fundador principal',
         'Seleccione su género',
         8, -- Gender
         11,
         0,
         0,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    DECLARE @Q1_11Id BIGINT = SCOPE_IDENTITY();
END

-- Question 1.12: Estado civil (MaritalStatus)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 12)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         'Estado civil del fundador',
         'Seleccione su estado civil',
         9, -- MaritalStatus
         12,
         0,
         0,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    DECLARE @Q1_12Id BIGINT = SCOPE_IDENTITY();
END

-- Question 1.13: Correo electrónico (Email)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 13)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         'Correo electrónico del fundador',
         'Ingrese un email válido',
         10, -- Email
         13,
         1,
         0,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    DECLARE @Q1_13Id BIGINT = SCOPE_IDENTITY();
END

-- Question 1.14: Número de teléfono (PhoneNumber)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 14)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         'Número de teléfono de contacto',
         'Incluya código de país',
         11, -- PhoneNumber
         14,
         1,
         0,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    DECLARE @Q1_14Id BIGINT = SCOPE_IDENTITY();
END

-- Question 1.15: Nacionalidad (Nationality)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 15)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId,
         @BlockInfoGeneralId,
         'Nacionalidad del fundador',
         'Seleccione su país de nacionalidad',
         12, -- Nationality
         15,
         0,
         0,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    DECLARE @Q1_15Id BIGINT = SCOPE_IDENTITY();
END

-- =============================================
-- Create ProjectQuestions for Block 2: Plan de Negocio (8 questions)
-- =============================================
DECLARE @Q2_1Id BIGINT, @Q2_2Id BIGINT, @Q2_3Id BIGINT, @Q2_4Id BIGINT, @Q2_5Id BIGINT, @Q2_6Id BIGINT, @Q2_7Id BIGINT, @Q2_8Id BIGINT;

-- Question 2.1: Propuesta de valor (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId,
         @BlockPlanNegocioId,
         '¿Cuál es su propuesta de valor única?',
         'Describa qué hace especial a su producto o servicio',
         3, -- FreeText
         1,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_1Id = SCOPE_IDENTITY();
END

-- Question 2.2: Modelo de ingresos (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId,
         @BlockPlanNegocioId,
         '¿Cuál es su principal modelo de ingresos?',
         'Seleccione el modelo que genera o generará la mayor parte de sus ingresos',
         1, -- SingleChoice
         2,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_2Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q2_2Id, 'Venta directa de productos', 3, 'F', 'Modelo tradicional probado', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Suscripción (SaaS)', 5, 'F', 'Ingresos recurrentes', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Comisiones', 3, 'O', 'Escalable', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Publicidad', 2, 'D', 'Depende del tráfico', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Freemium', 3, 'O', 'Potencial de conversión', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Marketplace', 4, 'O', 'Red de valor', 'N', 'Sin impacto ODS', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Licenciamiento', 4, 'F', 'Ingresos pasivos', 'N', 'Sin impacto ODS', 7, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 2.3: Canales de distribución (MultiChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId,
         @BlockPlanNegocioId,
         '¿Qué canales de distribución utiliza o planea utilizar?',
         'Seleccione todos los que apliquen',
         2, -- MultiChoice
         3,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_3Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q2_3Id, 'Venta directa', 3, 'F', 'Control total', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_3Id, 'E-commerce propio', 4, 'F', 'Escalable', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_3Id, 'Marketplaces', 3, 'O', 'Alcance amplio', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_3Id, 'Distribuidores', 3, 'O', 'Red establecida', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_3Id, 'Tiendas físicas', 3, 'F', 'Presencia local', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_3Id, 'App móvil', 4, 'F', 'Acceso directo', 'N', 'Sin impacto ODS', 6, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 2.4: Fecha de lanzamiento al mercado (Date)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 4)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId,
         @BlockPlanNegocioId,
         '¿Cuándo planea lanzar su producto al mercado?',
         'Si ya está en el mercado, indique la fecha de lanzamiento',
         5, -- Date
         4,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_4Id = SCOPE_IDENTITY();
END

-- Question 2.5: Inversión inicial requerida (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 5)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId,
         @BlockPlanNegocioId,
         '¿Cuál es el monto de inversión inicial requerida (en USD)?',
         'Ingrese el monto aproximado necesario para los próximos 12 meses',
         4, -- Numeric
         5,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_5Id = SCOPE_IDENTITY();
END

-- Question 2.6: Canvas completado (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 6)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId,
         @BlockPlanNegocioId,
         '¿Ha completado un modelo Canvas de su negocio?',
         'El modelo Canvas es una herramienta de gestión estratégica',
         1, -- SingleChoice
         6,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_6Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q2_6Id, 'Sí, completamente', 5, 'F', 'Planificación completa', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_6Id, 'Sí, parcialmente', 3, 'O', 'En proceso', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_6Id, 'No, pero conozco la herramienta', 2, 'D', 'Falta implementación', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_6Id, 'No conozco esta herramienta', 1, 'D', 'Requiere capacitación', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 2.7: Principales socios estratégicos (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 7)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId,
         @BlockPlanNegocioId,
         'Liste sus principales socios estratégicos o alianzas clave',
         'Mencione empresas, organizaciones o instituciones con las que colabora',
         3, -- FreeText
         7,
         0,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_7Id = SCOPE_IDENTITY();
END

-- Question 2.8: Tiempo para alcanzar punto de equilibrio (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 8)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId,
         @BlockPlanNegocioId,
         '¿En cuántos meses estima alcanzar el punto de equilibrio?',
         'Momento en que los ingresos igualan a los costos',
         4, -- Numeric
         8,
         1,
         1,
         2, -- Both (Start and Final)
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_8Id = SCOPE_IDENTITY();
END

-- =============================================
-- Create ProjectQuestions for Block 3: Análisis de Mercado (7 questions)
-- =============================================
DECLARE @Q3_1Id BIGINT, @Q3_2Id BIGINT, @Q3_3Id BIGINT, @Q3_4Id BIGINT, @Q3_5Id BIGINT, @Q3_6Id BIGINT, @Q3_7Id BIGINT;

-- Question 3.1: Tamaño del mercado (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId,
         @BlockAnalisisMercadoId,
         '¿Cuál es el tamaño estimado de su mercado objetivo?',
         'Considere su mercado direccionable total (TAM)',
         1, -- SingleChoice
         1,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_1Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q3_1Id, 'Menos de $1 millón USD', 1, 'D', 'Mercado pequeño', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, '$1-10 millones USD', 2, 'O', 'Mercado nicho', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, '$10-100 millones USD', 3, 'O', 'Mercado mediano', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, '$100 millones - $1 billón USD', 4, 'O', 'Mercado grande', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, 'Más de $1 billón USD', 5, 'O', 'Mercado masivo', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 3.2: Cliente objetivo (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId,
         @BlockAnalisisMercadoId,
         'Describa su cliente objetivo principal',
         'Incluya demografía, necesidades y comportamientos',
         3, -- FreeText
         2,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_2Id = SCOPE_IDENTITY();
END

-- Question 3.3: Estrategias de marketing (MultiChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId,
         @BlockAnalisisMercadoId,
         '¿Qué estrategias de marketing utiliza o planea utilizar?',
         'Seleccione todas las que apliquen',
         2, -- MultiChoice
         3,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_3Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q3_3Id, 'Marketing digital', 3, 'F', 'Alcance amplio', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_3Id, 'Redes sociales', 3, 'F', 'Engagement directo', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_3Id, 'SEO/SEM', 4, 'F', 'Tráfico orgánico', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_3Id, 'Email marketing', 3, 'F', 'Comunicación directa', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_3Id, 'Marketing de contenidos', 4, 'F', 'Valor agregado', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_3Id, 'Publicidad tradicional', 2, 'D', 'Alto costo', 'N', 'Sin impacto ODS', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_3Id, 'Eventos y ferias', 3, 'O', 'Networking', 'N', 'Sin impacto ODS', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_3Id, 'Marketing de influencers', 3, 'O', 'Alcance segmentado', 'N', 'Sin impacto ODS', 8, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 3.4: Competidores principales (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 4)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId,
         @BlockAnalisisMercadoId,
         '¿Cuántos competidores principales ha identificado?',
         'Considere competidores directos e indirectos',
         4, -- Numeric
         4,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_4Id = SCOPE_IDENTITY();
END

-- Question 3.5: Fecha de primer cliente (Date)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 5)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId,
         @BlockAnalisisMercadoId,
         '¿Cuándo obtuvo su primer cliente?',
         'Si aún no tiene clientes, indique la fecha esperada',
         5, -- Date
         5,
         0,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_5Id = SCOPE_IDENTITY();
END

-- Question 3.6: Validación con clientes (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 6)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId,
         @BlockAnalisisMercadoId,
         '¿Ha validado su propuesta con clientes potenciales?',
         'Validación mediante entrevistas, encuestas o ventas piloto',
         1, -- SingleChoice
         6,
         1,
         1,
         2, -- Both
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_6Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q3_6Id, 'Sí, con más de 50 clientes', 5, 'F', 'Validación sólida', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_6Id, 'Sí, con 10-50 clientes', 3, 'F', 'Validación moderada', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_6Id, 'Sí, con menos de 10 clientes', 2, 'D', 'Validación limitada', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_6Id, 'No, aún no hemos validado', 1, 'D', 'Sin validación', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 3.7: Métricas de éxito (FreeText) - Final phase question
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 7)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId,
         @BlockAnalisisMercadoId,
         '¿Cuáles han sido sus principales métricas de éxito hasta ahora?',
         'Incluya KPIs, ventas, usuarios, o cualquier indicador relevante',
         3, -- FreeText
         7,
         0,
         1,
         1, -- Final
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_7Id = SCOPE_IDENTITY();
END

-- =============================================
-- Create ProjectQuestions for Block 4: Análisis Financiero (8 questions)
-- =============================================
DECLARE @Q4_1Id BIGINT, @Q4_2Id BIGINT, @Q4_3Id BIGINT, @Q4_4Id BIGINT, @Q4_5Id BIGINT, @Q4_6Id BIGINT, @Q4_7Id BIGINT, @Q4_8Id BIGINT;

-- Question 4.1: Ingresos actuales mensuales (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinanzasId,
         @BlockFinancieroId,
         '¿Cuáles son sus ingresos mensuales actuales (en USD)?',
         'Si no tiene ingresos aún, ingrese 0',
         4, -- Numeric
         1,
         1,
         1,
         2, -- Both
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_1Id = SCOPE_IDENTITY();
END

-- Question 4.2: Fuentes de financiamiento (MultiChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinanzasId,
         @BlockFinancieroId,
         '¿Qué fuentes de financiamiento ha utilizado o planea utilizar?',
         'Seleccione todas las que apliquen',
         2, -- MultiChoice
         2,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_2Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q4_2Id, 'Recursos propios', 3, 'F', 'Independencia', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Familiares y amigos', 2, 'D', 'Recursos limitados', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Inversionistas ángeles', 4, 'O', 'Capital inteligente', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Capital de riesgo', 5, 'O', 'Escalamiento rápido', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Préstamos bancarios', 3, 'F', 'Acceso a capital', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Crowdfunding', 3, 'O', 'Validación de mercado', 'N', 'Sin impacto ODS', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Subsidios gubernamentales', 4, 'O', 'Apoyo institucional', 'N', 'Sin impacto ODS', 7, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 4.3: Fecha de última ronda de inversión (Date)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinanzasId,
         @BlockFinancieroId,
         '¿Cuándo fue su última ronda de inversión?',
         'Si no ha tenido rondas de inversión, deje en blanco',
         5, -- Date
         3,
         0,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_3Id = SCOPE_IDENTITY();
END

-- Question 4.4: Gastos mensuales operativos (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 4)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinanzasId,
         @BlockFinancieroId,
         '¿Cuáles son sus gastos operativos mensuales (en USD)?',
         'Incluya todos los costos fijos y variables',
         4, -- Numeric
         4,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_4Id = SCOPE_IDENTITY();
END

-- Question 4.5: Modelo de proyección financiera (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 5)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinanzasId,
         @BlockFinancieroId,
         '¿Tiene un modelo de proyección financiera desarrollado?',
         'Proyecciones a 3-5 años',
         1, -- SingleChoice
         5,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_5Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q4_5Id, 'Sí, detallado y validado', 5, 'F', 'Planificación sólida', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_5Id, 'Sí, básico', 3, 'O', 'Requiere mejora', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_5Id, 'En desarrollo', 2, 'D', 'En proceso', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_5Id, 'No tenemos', 1, 'D', 'Falta planificación', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 4.6: Margen de utilidad esperado (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 6)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinanzasId,
         @BlockFinancieroId,
         '¿Cuál es su margen de utilidad neta esperado (%)?',
         'Porcentaje de utilidad después de todos los gastos',
         4, -- Numeric
         6,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_6Id = SCOPE_IDENTITY();
END

-- Question 4.7: Estrategia de salida (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 7)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinanzasId,
         @BlockFinancieroId,
         '¿Cuál es su estrategia de salida preferida?',
         'Plan a largo plazo para inversionistas',
         1, -- SingleChoice
         7,
         0,
         1,
         1, -- Final
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_7Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q4_7Id, 'IPO (Oferta pública)', 5, 'O', 'Máximo valor', 'N', 'Sin impacto ODS', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_7Id, 'Adquisición estratégica', 4, 'O', 'Salida rápida', 'N', 'Sin impacto ODS', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_7Id, 'Fusión', 3, 'O', 'Consolidación', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_7Id, 'Mantener como negocio familiar', 3, 'F', 'Control total', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_7Id, 'No he considerado esto', 1, 'D', 'Falta planificación', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 4.8: Riesgos financieros identificados (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 8)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinanzasId,
         @BlockFinancieroId,
         'Describa los principales riesgos financieros que ha identificado',
         'Incluya estrategias de mitigación si las tiene',
         3, -- FreeText
         8,
         0,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_8Id = SCOPE_IDENTITY();
END

-- =============================================
-- Create ProjectQuestions for Block 5: Innovación y Tecnología (7 questions)
-- =============================================
DECLARE @Q5_1Id BIGINT, @Q5_2Id BIGINT, @Q5_3Id BIGINT, @Q5_4Id BIGINT, @Q5_5Id BIGINT, @Q5_6Id BIGINT, @Q5_7Id BIGINT;

-- Question 5.1: Nivel de innovación (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId,
         @BlockInnovacionId,
         '¿Cómo calificaría el nivel de innovación de su producto/servicio?',
         'Compare con soluciones existentes en el mercado',
         1, -- SingleChoice
         1,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_1Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q5_1Id, 'Disruptivo - Cambia el paradigma del mercado', 5, 'F', 'Innovación radical', 'S', 'Innovación', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'Altamente innovador - Nuevas características únicas', 4, 'F', 'Ventaja competitiva', 'S', 'Innovación', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'Moderadamente innovador - Mejoras sobre lo existente', 3, 'O', 'Diferenciación', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'Poco innovador - Similar a lo existente', 2, 'D', 'Poca diferenciación', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'No innovador - Copia de lo existente', 1, 'D', 'Sin diferenciación', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 5.2: Tecnologías utilizadas (MultiChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId,
         @BlockInnovacionId,
         '¿Qué tecnologías emergentes utiliza o planea utilizar?',
         'Seleccione todas las que apliquen',
         2, -- MultiChoice
         2,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_2Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q5_2Id, 'Inteligencia Artificial', 5, 'F', 'Tecnología de vanguardia', 'S', 'Innovación', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Machine Learning', 5, 'F', 'Análisis avanzado', 'S', 'Innovación', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Blockchain', 4, 'O', 'Descentralización', 'S', 'Innovación', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'IoT (Internet de las Cosas)', 4, 'F', 'Conectividad', 'S', 'Innovación', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Realidad Virtual/Aumentada', 4, 'O', 'Experiencia inmersiva', 'S', 'Innovación', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Cloud Computing', 3, 'F', 'Escalabilidad', 'N', 'Sin impacto ODS', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Big Data', 4, 'F', 'Análisis de datos', 'S', 'Innovación', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Ninguna de las anteriores', 1, 'D', 'Sin tecnología emergente', 'N', 'Sin impacto ODS', 8, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 5.3: Inversión en I+D (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId,
         @BlockInnovacionId,
         '¿Qué porcentaje de sus ingresos invierte en I+D?',
         'Si no tiene ingresos, indique el porcentaje del presupuesto',
         4, -- Numeric
         3,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_3Id = SCOPE_IDENTITY();
END

-- Question 5.4: Fecha de último desarrollo tecnológico (Date)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 4)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId,
         @BlockInnovacionId,
         '¿Cuándo implementó su último desarrollo tecnológico significativo?',
         'Fecha de la última actualización o mejora importante',
         5, -- Date
         4,
         0,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_4Id = SCOPE_IDENTITY();
END

-- Question 5.5: Propiedad intelectual (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 5)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId,
         @BlockInnovacionId,
         '¿Tiene protección de propiedad intelectual?',
         'Patentes, marcas registradas, derechos de autor',
         1, -- SingleChoice
         5,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_5Id = SCOPE_IDENTITY();
    
    -- Add answer options
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q5_5Id, 'Sí, patentes otorgadas', 5, 'F', 'Protección total', 'S', 'Innovación', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_5Id, 'Sí, patentes en proceso', 4, 'F', 'Protección en curso', 'S', 'Innovación', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_5Id, 'Sí, marca registrada', 3, 'F', 'Protección de marca', 'N', 'Sin impacto ODS', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_5Id, 'En proceso de registro', 2, 'O', 'Protección futura', 'N', 'Sin impacto ODS', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_5Id, 'No tenemos', 1, 'D', 'Sin protección', 'N', 'Sin impacto ODS', 5, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 5.6: Impacto ambiental (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 6)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId,
         @BlockInnovacionId,
         'Describa el impacto ambiental de su solución tecnológica',
         'Incluya aspectos positivos y negativos',
         3, -- FreeText
         6,
         0,
         1,
         1, -- Final
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_6Id = SCOPE_IDENTITY();
END

-- Question 5.7: Plan de escalamiento tecnológico (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 7)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId,
         @BlockInnovacionId,
         'Describa su plan de escalamiento tecnológico para los próximos 2 años',
         'Incluya infraestructura, equipo técnico y desarrollos planificados',
         3, -- FreeText
         7,
         0,
         1,
         1, -- Final
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_7Id = SCOPE_IDENTITY();
END

-- =============================================
-- Block 6: Información Personal (Specialized Types)
-- =============================================
DECLARE @BlockInfoPersonalId BIGINT;

IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId AND Name = 'Información Personal')
BEGIN
    INSERT INTO [businessincubators].[ProjectBlocks]
        (ProjectId, Name, IsNameCustomized)
    VALUES
        (@DemoProjectId,
         'Información Personal',
         0);
    
    SET @BlockInfoPersonalId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Block 6: Información Personal';
END
ELSE
BEGIN
    SELECT @BlockInfoPersonalId = Id FROM [businessincubators].[ProjectBlocks] 
    WHERE ProjectId = @DemoProjectId AND Name = 'Información Personal';
END

-- Topic for personal information
DECLARE @TopicInfoPersonalId BIGINT;

IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleGeneralId AND Name = 'Datos Personales')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, [Order], IsOrderCustomized, IsNameCustomized)
    VALUES
        (@ModuleGeneralId,
         'Datos Personales',
         6,
         0, 0);
    
    SET @TopicInfoPersonalId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @TopicInfoPersonalId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleGeneralId AND Name = 'Datos Personales';
END

-- Question 6.1: Número de identificación (PersonId)
DECLARE @Q6_1Id BIGINT;
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoPersonalId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoPersonalId,
         @BlockInfoPersonalId,
         '¿Cuál es su número de identificación?',
         'Ingrese su número de documento de identidad',
         6, -- PersonId
         1,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q6_1Id = SCOPE_IDENTITY();
END

-- Question 6.2: Tipo de documento (IdType)
DECLARE @Q6_2Id BIGINT;
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoPersonalId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoPersonalId,
         @BlockInfoPersonalId,
         '¿Cuál es su tipo de documento?',
         'Seleccione el tipo de documento de identidad',
         7, -- IdType
         2,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q6_2Id = SCOPE_IDENTITY();
END

-- Question 6.3: Género (Gender)
DECLARE @Q6_3Id BIGINT;
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoPersonalId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoPersonalId,
         @BlockInfoPersonalId,
         '¿Cuál es su género?',
         'Seleccione su género',
         8, -- Gender
         3,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q6_3Id = SCOPE_IDENTITY();
END

-- Question 6.4: Estado civil (MaritalStatus)
DECLARE @Q6_4Id BIGINT;
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoPersonalId AND [Order] = 4)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoPersonalId,
         @BlockInfoPersonalId,
         '¿Cuál es su estado civil?',
         'Seleccione su estado civil actual',
         9, -- MaritalStatus
         4,
         0,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q6_4Id = SCOPE_IDENTITY();
END

-- Question 6.5: Correo electrónico (Email)
DECLARE @Q6_5Id BIGINT;
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoPersonalId AND [Order] = 5)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoPersonalId,
         @BlockInfoPersonalId,
         '¿Cuál es su correo electrónico de contacto?',
         'Ingrese un correo electrónico válido',
         10, -- Email
         5,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q6_5Id = SCOPE_IDENTITY();
END

-- Question 6.6: Número de teléfono (PhoneNumber)
DECLARE @Q6_6Id BIGINT;
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoPersonalId AND [Order] = 6)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoPersonalId,
         @BlockInfoPersonalId,
         '¿Cuál es su número de teléfono?',
         'Ingrese su número de teléfono con código de área',
         11, -- PhoneNumber
         6,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q6_6Id = SCOPE_IDENTITY();
END

-- Question 6.7: Nacionalidad (Nationality)
DECLARE @Q6_7Id BIGINT;
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoPersonalId AND [Order] = 7)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoPersonalId,
         @BlockInfoPersonalId,
         '¿Cuál es su nacionalidad?',
         'Seleccione su país de nacionalidad',
         12, -- Nationality
         7,
         1,
         1,
         0, -- Start
         0,
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q6_7Id = SCOPE_IDENTITY();
END

-- =============================================
-- Verification Queries
-- =============================================
PRINT '';
PRINT '=== VERIFICATION RESULTS ===';

-- Verify ProjectKnowledgeStructure
IF EXISTS (SELECT 1 FROM [businessincubators].[ProjectKnowledgeStructures] WHERE ProjectId = @DemoProjectId)
BEGIN
    PRINT '✓ ProjectKnowledgeStructure created successfully';
END
ELSE
BEGIN
    PRINT '✗ ERROR: ProjectKnowledgeStructure not found';
END

-- Verify ProjectBlocks
DECLARE @BlockCount INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId);
PRINT '✓ ProjectBlocks created: ' + CAST(@BlockCount AS NVARCHAR(10)) + ' blocks (expected: 6)';

-- Verify ProjectQuestions
DECLARE @QuestionCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId
);
PRINT '✓ ProjectQuestions created: ' + CAST(@QuestionCount AS NVARCHAR(10)) + ' questions (expected: 52)';

-- Verify questions by type
DECLARE @SingleChoiceCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 1
);
DECLARE @MultiChoiceCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 2
);
DECLARE @FreeTextCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 3
);
DECLARE @NumericCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 4
);
DECLARE @DateCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 5
);

-- Count specialized types
DECLARE @PersonIdCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 6
);
DECLARE @IdTypeCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 7
);
DECLARE @GenderCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 8
);
DECLARE @MaritalStatusCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 9
);
DECLARE @EmailCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 10
);
DECLARE @PhoneNumberCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 11
);
DECLARE @NationalityCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AnswerType = 12
);

PRINT '';
PRINT '  Question Types Distribution:';
PRINT '  Basic Types:';
PRINT '  - SingleChoice: ' + CAST(@SingleChoiceCount AS NVARCHAR(10)) + ' questions';
PRINT '  - MultiChoice: ' + CAST(@MultiChoiceCount AS NVARCHAR(10)) + ' questions';
PRINT '  - FreeText: ' + CAST(@FreeTextCount AS NVARCHAR(10)) + ' questions';
PRINT '  - Numeric: ' + CAST(@NumericCount AS NVARCHAR(10)) + ' questions';
PRINT '  - Date: ' + CAST(@DateCount AS NVARCHAR(10)) + ' questions';
PRINT '  Specialized Types:';
PRINT '  - PersonId: ' + CAST(@PersonIdCount AS NVARCHAR(10)) + ' questions';
PRINT '  - IdType: ' + CAST(@IdTypeCount AS NVARCHAR(10)) + ' questions';
PRINT '  - Gender: ' + CAST(@GenderCount AS NVARCHAR(10)) + ' questions';
PRINT '  - MaritalStatus: ' + CAST(@MaritalStatusCount AS NVARCHAR(10)) + ' questions';
PRINT '  - Email: ' + CAST(@EmailCount AS NVARCHAR(10)) + ' questions';
PRINT '  - PhoneNumber: ' + CAST(@PhoneNumberCount AS NVARCHAR(10)) + ' questions';
PRINT '  - Nationality: ' + CAST(@NationalityCount AS NVARCHAR(10)) + ' questions';

-- Verify questions by phase
DECLARE @StartQuestions INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AppliesToPhase IN (0, 2) -- 0=Start, 2=Both
);
DECLARE @FinalQuestions INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.AppliesToPhase IN (1, 2) -- 1=Final, 2=Both
);
PRINT '';
PRINT '  Questions by Phase:';
PRINT '  - Questions for Start phase: ' + CAST(@StartQuestions AS NVARCHAR(10));
PRINT '  - Questions for Final phase: ' + CAST(@FinalQuestions AS NVARCHAR(10));

-- Verify ProjectAnswerOptions
DECLARE @OptionCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectAnswerOptions] o
    INNER JOIN [businessincubators].[ProjectQuestions] q ON o.ProjectQuestionId = q.Id
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId
);
PRINT '';
PRINT '✓ ProjectAnswerOptions created: ' + CAST(@OptionCount AS NVARCHAR(10)) + ' options';

-- Verify questions with diagnosis flag
DECLARE @DiagnosisQuestions INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId AND q.IsUsedForDiagnosis = 1
);
PRINT '✓ Questions marked for diagnosis: ' + CAST(@DiagnosisQuestions AS NVARCHAR(10));

-- Summary per block
PRINT '';
PRINT '  Questions per Block:';
DECLARE @Block1Count INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId);
DECLARE @Block2Count INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId);
DECLARE @Block3Count INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId);
DECLARE @Block4Count INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId);
DECLARE @Block5Count INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId);

PRINT '  - Block 1 (Información General): ' + CAST(@Block1Count AS NVARCHAR(10)) + ' questions';
PRINT '  - Block 2 (Plan de Negocio): ' + CAST(@Block2Count AS NVARCHAR(10)) + ' questions';
PRINT '  - Block 3 (Análisis de Mercado): ' + CAST(@Block3Count AS NVARCHAR(10)) + ' questions';
PRINT '  - Block 4 (Análisis Financiero): ' + CAST(@Block4Count AS NVARCHAR(10)) + ' questions';
PRINT '  - Block 5 (Innovación y Tecnología): ' + CAST(@Block5Count AS NVARCHAR(10)) + ' questions';

PRINT '';
PRINT '[011.SeedProjectKnowledgeStructure.sql] Completed successfully!';
PRINT 'demo.starter user should now be able to see a comprehensive form with 5 blocks on the Participant Dashboard.';

GO