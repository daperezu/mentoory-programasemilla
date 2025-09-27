-- =============================================
-- Seed Data for Project Knowledge Structure with Enhanced FODA/ODSR Coverage
-- Created: 2025-09-06
-- Updated: 2025-09-10
-- Description: Creates complete hierarchy with comprehensive FODA/ODSR types and scoring
-- Changes:
--   - Questions in "Información General" block: IsUsedForDiagnosis = 0, AppliesToPhase = Start
--   - All other blocks: IsUsedForDiagnosis = 1, AppliesToPhase = Both
--   - Complete FODA coverage (F, O, D, A, N) across answer options
--   - Complete ODSR coverage (O, D, S, R, N) across answer options
--   - Progressive scoring from 1 to N for answer options
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
         'Estructura completa para evaluar proyectos de innovación tecnológica con análisis FODA y ODSR',
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
-- Create ProjectModules
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
        (@KnowledgeStructureId, 'Módulo General', 0, 1, 0);
    
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
        (@KnowledgeStructureId, 'Módulo de Negocio', 0, 2, 0);
    
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
        (@KnowledgeStructureId, 'Módulo de Mercado', 0, 3, 0);
    
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
        (@KnowledgeStructureId, 'Módulo Financiero', 0, 4, 0);
    
    SET @ModuleFinancieroId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Module: Módulo Financiero';
END
ELSE
BEGIN
    SELECT @ModuleFinancieroId = Id FROM [businessincubators].[ProjectModules] 
    WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo Financiero';
END

-- Module 5: Módulo de Innovación
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectModules] WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo de Innovación')
BEGIN
    INSERT INTO [businessincubators].[ProjectModules]
        (ProjectKnowledgeStructureId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@KnowledgeStructureId, 'Módulo de Innovación', 0, 5, 0);
    
    SET @ModuleInnovacionId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Module: Módulo de Innovación';
END
ELSE
BEGIN
    SELECT @ModuleInnovacionId = Id FROM [businessincubators].[ProjectModules] 
    WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId AND Name = 'Módulo de Innovación';
END

-- =============================================
-- Create ProjectTopics
-- =============================================
DECLARE @TopicInfoGeneralId BIGINT;
DECLARE @TopicPlanNegocioId BIGINT;
DECLARE @TopicAnalisisMercadoId BIGINT;
DECLARE @TopicFinancieroId BIGINT;
DECLARE @TopicInnovacionId BIGINT;

-- Topic for Módulo General
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleGeneralId AND Name = 'Información Básica')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleGeneralId, 'Información Básica', 0, 1, 0);
    
    SET @TopicInfoGeneralId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Información Básica';
END
ELSE
BEGIN
    SELECT @TopicInfoGeneralId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleGeneralId AND Name = 'Información Básica';
END

-- Topic for Módulo de Negocio
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleNegocioId AND Name = 'Modelo de Negocio')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleNegocioId, 'Modelo de Negocio', 0, 1, 0);
    
    SET @TopicPlanNegocioId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Modelo de Negocio';
END
ELSE
BEGIN
    SELECT @TopicPlanNegocioId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleNegocioId AND Name = 'Modelo de Negocio';
END

-- Topic for Módulo de Mercado
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleMercadoId AND Name = 'Estrategia de Mercado')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleMercadoId, 'Estrategia de Mercado', 0, 1, 0);
    
    SET @TopicAnalisisMercadoId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Estrategia de Mercado';
END
ELSE
BEGIN
    SELECT @TopicAnalisisMercadoId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleMercadoId AND Name = 'Estrategia de Mercado';
END

-- Topic for Módulo Financiero
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleFinancieroId AND Name = 'Análisis Financiero')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleFinancieroId, 'Análisis Financiero', 0, 1, 0);
    
    SET @TopicFinancieroId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Análisis Financiero';
END
ELSE
BEGIN
    SELECT @TopicFinancieroId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleFinancieroId AND Name = 'Análisis Financiero';
END

-- Topic for Módulo de Innovación
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectTopics] WHERE ProjectModuleId = @ModuleInnovacionId AND Name = 'Tecnología e Innovación')
BEGIN
    INSERT INTO [businessincubators].[ProjectTopics]
        (ProjectModuleId, Name, IsNameCustomized, [Order], IsOrderCustomized)
    VALUES
        (@ModuleInnovacionId, 'Tecnología e Innovación', 0, 1, 0);
    
    SET @TopicInnovacionId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Topic: Tecnología e Innovación';
END
ELSE
BEGIN
    SELECT @TopicInnovacionId = Id FROM [businessincubators].[ProjectTopics] 
    WHERE ProjectModuleId = @ModuleInnovacionId AND Name = 'Tecnología e Innovación';
END

-- =============================================
-- Create ProjectBlocks
-- =============================================
DECLARE @BlockInfoGeneralId BIGINT;
DECLARE @BlockPlanNegocioId BIGINT;
DECLARE @BlockAnalisisMercadoId BIGINT;
DECLARE @BlockFinancieroId BIGINT;
DECLARE @BlockInnovacionId BIGINT;

-- Block 1: Información General
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId AND Name = 'Información General')
BEGIN
    INSERT INTO [businessincubators].[ProjectBlocks]
        (ProjectId, Name, IsNameCustomized)
    VALUES
        (@DemoProjectId, 'Información General', 0);
    
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
        (@DemoProjectId, 'Plan de Negocio', 0);
    
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
        (@DemoProjectId, 'Análisis de Mercado', 0);
    
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
        (@DemoProjectId, 'Análisis Financiero', 0);
    
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
        (@DemoProjectId, 'Innovación y Tecnología', 0);
    
    SET @BlockInnovacionId = SCOPE_IDENTITY();
    PRINT '[011.SeedProjectKnowledgeStructure.sql] Created Block: Innovación y Tecnología';
END
ELSE
BEGIN
    SELECT @BlockInnovacionId = Id FROM [businessincubators].[ProjectBlocks] 
    WHERE ProjectId = @DemoProjectId AND Name = 'Innovación y Tecnología';
END

-- =============================================
-- Create ProjectQuestions for Block 1: Información General
-- IMPORTANT: IsUsedForDiagnosis = 0, AppliesToPhase = 0 (Start)
-- =============================================
DECLARE @Q1_1Id BIGINT, @Q1_2Id BIGINT, @Q1_3Id BIGINT, @Q1_4Id BIGINT, @Q1_5Id BIGINT;

-- Question 1.1: Nombre del emprendimiento (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan, 
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId, @BlockInfoGeneralId,
         '¿Cuál es el nombre de su emprendimiento?',
         'Ingrese el nombre comercial o marca de su proyecto',
         3, -- FreeText
         1, 1,
         0, -- IsUsedForDiagnosis = 0 for Información General
         0, -- AppliesToPhase = Start
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_1Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q1_1Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 1;
END

-- Question 1.2: Fecha de fundación (Date)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized, 
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId, @BlockInfoGeneralId,
         '¿Cuándo fue fundado su emprendimiento?',
         'Seleccione la fecha de inicio de su proyecto',
         5, -- Date
         2, 1,
         0, -- IsUsedForDiagnosis = 0 for Información General
         0, -- AppliesToPhase = Start
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_2Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q1_2Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 2;
END

-- Question 1.3: Etapa del emprendimiento (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId, @BlockInfoGeneralId,
         '¿En qué etapa se encuentra su emprendimiento?',
         'Seleccione la etapa actual de desarrollo',
         1, -- SingleChoice
         3, 1,
         0, -- IsUsedForDiagnosis = 0 for Información General
         0, -- AppliesToPhase = Start
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_3Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q1_3Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 3;
END

-- Answer options for Q1.3 (No FODA/ODSR since IsUsedForDiagnosis = 0)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q1_3Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q1_3Id, 'Idea', 1, 'N', '', 'N', '', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_3Id, 'Prototipo', 2, 'N', '', 'N', '', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_3Id, 'MVP (Producto Mínimo Viable)', 3, 'N', '', 'N', '', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_3Id, 'Producto en el mercado', 4, 'N', '', 'N', '', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_3Id, 'Escalamiento', 5, 'N', '', 'N', '', 5, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 1.4: Número de empleados (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 4)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId, @BlockInfoGeneralId,
         '¿Cuántos empleados tiene actualmente?',
         'Ingrese el número total de empleados',
         4, -- Numeric
         4, 1,
         0, -- IsUsedForDiagnosis = 0 for Información General
         0, -- AppliesToPhase = Start
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_4Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q1_4Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 4;
END

-- Question 1.5: Sector de la industria (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 5)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInfoGeneralId, @BlockInfoGeneralId,
         '¿En qué sector opera su emprendimiento?',
         'Seleccione el sector principal de su negocio',
         1, -- SingleChoice
         5, 1,
         0, -- IsUsedForDiagnosis = 0 for Información General
         0, -- AppliesToPhase = Start
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q1_5Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q1_5Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInfoGeneralId AND [Order] = 5;
END

-- Answer options for Q1.5 (No FODA/ODSR since IsUsedForDiagnosis = 0)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q1_5Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q1_5Id, 'Tecnología', 1, 'N', '', 'N', '', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Salud', 2, 'N', '', 'N', '', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Educación', 3, 'N', '', 'N', '', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Finanzas', 4, 'N', '', 'N', '', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Comercio', 5, 'N', '', 'N', '', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Manufactura', 6, 'N', '', 'N', '', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Servicios', 7, 'N', '', 'N', '', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q1_5Id, 'Agricultura', 8, 'N', '', 'N', '', 8, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- =============================================
-- Create ProjectQuestions for Block 2: Plan de Negocio
-- IMPORTANT: IsUsedForDiagnosis = 1, AppliesToPhase = 2 (Both)
-- Rich FODA/ODSR coverage
-- =============================================
DECLARE @Q2_1Id BIGINT, @Q2_2Id BIGINT, @Q2_3Id BIGINT, @Q2_4Id BIGINT, @Q2_5Id BIGINT;

-- Question 2.1: Modelo de negocio (SingleChoice with FODA/ODSR)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId, @BlockPlanNegocioId,
         '¿Cuál es su modelo de negocio principal?',
         'Seleccione cómo genera ingresos su emprendimiento',
         1, -- SingleChoice
         1, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_1Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q2_1Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 1;
END

-- Answer options for Q2.1 with complete FODA/ODSR coverage
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q2_1Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        -- Complete FODA coverage: F, O, D, A, and varied ODSR: O, D, S, R
        (@Q2_1Id, 'B2B (Negocio a Negocio)', 5, 'F', 'Modelo sólido con clientes empresariales', 'O', 'Estrategia ofensiva de expansión', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_1Id, 'B2C (Negocio a Consumidor)', 4, 'O', 'Gran potencial de mercado masivo', 'D', 'Requiere defensa de posición', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_1Id, 'SaaS (Software como Servicio)', 5, 'F', 'Ingresos recurrentes predecibles', 'O', 'Crecimiento agresivo posible', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_1Id, 'Marketplace', 3, 'D', 'Dependencia de múltiples actores', 'S', 'Supervivencia en mercado competitivo', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_1Id, 'Freemium', 2, 'A', 'Conversión a pago puede ser baja', 'R', 'Requiere reorientación del modelo', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_1Id, 'Suscripción', 5, 'F', 'Flujo de caja estable', 'O', 'Expansión de servicios posible', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_1Id, 'Comisiones', 3, 'O', 'Escalable sin inventario', 'D', 'Proteger márgenes de comisión', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_1Id, 'Publicidad', 1, 'A', 'Modelo vulnerable a cambios de mercado', 'S', 'Supervivencia difícil sin escala', 8, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 2.2: Propuesta de valor (MultiChoice with FODA/ODSR)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId, @BlockPlanNegocioId,
         '¿Cuáles son los elementos clave de su propuesta de valor?',
         'Seleccione todos los que apliquen',
         2, -- MultiChoice
         2, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_2Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q2_2Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 2;
END

-- Answer options for Q2.2 with varied FODA/ODSR
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q2_2Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q2_2Id, 'Innovación tecnológica', 5, 'F', 'Diferenciación por tecnología', 'O', 'Liderazgo tecnológico', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Precio competitivo', 3, 'D', 'Competencia por precio es vulnerable', 'D', 'Defensa de márgenes necesaria', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Calidad superior', 4, 'F', 'Ventaja competitiva sostenible', 'O', 'Expansión por calidad', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Experiencia del cliente', 5, 'O', 'Oportunidad de fidelización', 'O', 'Crecimiento por referidos', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Sustentabilidad', 4, 'O', 'Tendencia creciente del mercado', 'R', 'Reorientación hacia impacto', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Personalización', 3, 'F', 'Adaptación a necesidades específicas', 'D', 'Defender nicho de mercado', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Rapidez en entrega', 2, 'A', 'Logística puede ser un reto', 'S', 'Supervivencia operacional crítica', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q2_2Id, 'Servicio postventa', 4, 'F', 'Genera lealtad del cliente', 'O', 'Oportunidad de ventas cruzadas', 8, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 2.3: Descripción del producto/servicio (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicPlanNegocioId, @BlockPlanNegocioId,
         'Describa su producto o servicio principal',
         'Explique en detalle qué ofrece su emprendimiento',
         3, -- FreeText
         3, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q2_3Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q2_3Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockPlanNegocioId AND [Order] = 3;
END

-- =============================================
-- Create ProjectQuestions for Block 3: Análisis de Mercado
-- IMPORTANT: IsUsedForDiagnosis = 1, AppliesToPhase = 2 (Both)
-- =============================================
DECLARE @Q3_1Id BIGINT, @Q3_2Id BIGINT, @Q3_3Id BIGINT, @Q3_4Id BIGINT;

-- Question 3.1: Tamaño del mercado objetivo (SingleChoice with FODA/ODSR)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId, @BlockAnalisisMercadoId,
         '¿Cuál es el tamaño estimado de su mercado objetivo?',
         'Seleccione el rango de mercado potencial',
         1, -- SingleChoice
         1, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_1Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q3_1Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 1;
END

-- Answer options for Q3.1 with diverse FODA/ODSR combinations
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q3_1Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q3_1Id, 'Menos de $100K USD', 1, 'A', 'Mercado muy limitado', 'S', 'Estrategia de supervivencia', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, '$100K - $1M USD', 2, 'D', 'Mercado pequeño con retos', 'R', 'Requiere reorientación', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, '$1M - $10M USD', 3, 'O', 'Mercado con potencial', 'D', 'Defender posición inicial', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, '$10M - $100M USD', 4, 'F', 'Mercado atractivo', 'O', 'Oportunidad de crecimiento', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, '$100M - $1B USD', 5, 'F', 'Mercado grande y escalable', 'O', 'Expansión agresiva viable', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_1Id, 'Más de $1B USD', 6, 'O', 'Mercado masivo global', 'O', 'Dominio de mercado posible', 6, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 3.2: Competencia (MultiChoice with FODA/ODSR)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicAnalisisMercadoId, @BlockAnalisisMercadoId,
         '¿Cómo se posiciona frente a la competencia?',
         'Seleccione todas las ventajas competitivas que posee',
         2, -- MultiChoice
         2, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q3_2Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q3_2Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockAnalisisMercadoId AND [Order] = 2;
END

-- Answer options for Q3.2 cycling through all FODA types
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q3_2Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q3_2Id, 'Primer jugador en el mercado', 5, 'F', 'Ventaja del pionero', 'O', 'Captura temprana de mercado', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_2Id, 'Mejor tecnología', 5, 'F', 'Superioridad tecnológica', 'O', 'Liderazgo por innovación', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_2Id, 'Mejor precio', 2, 'A', 'Guerra de precios es riesgosa', 'S', 'Supervivencia por volumen', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_2Id, 'Mejor servicio', 4, 'O', 'Diferenciación por servicio', 'D', 'Defender calidad de servicio', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_2Id, 'Red de distribución', 4, 'F', 'Infraestructura establecida', 'O', 'Expansión geográfica', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_2Id, 'Marca reconocida', 3, 'D', 'Marca en construcción', 'R', 'Reposicionamiento de marca', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_2Id, 'Patentes/Propiedad intelectual', 5, 'F', 'Protección legal fuerte', 'D', 'Defender propiedad intelectual', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q3_2Id, 'Alianzas estratégicas', 4, 'O', 'Acceso a recursos externos', 'O', 'Crecimiento por alianzas', 8, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- =============================================
-- Create ProjectQuestions for Block 4: Análisis Financiero
-- IMPORTANT: IsUsedForDiagnosis = 1, AppliesToPhase = 2 (Both)
-- =============================================
DECLARE @Q4_1Id BIGINT, @Q4_2Id BIGINT, @Q4_3Id BIGINT, @Q4_4Id BIGINT;

-- Question 4.1: Estado financiero actual (SingleChoice with FODA/ODSR)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinancieroId, @BlockFinancieroId,
         '¿Cuál es el estado financiero actual de su emprendimiento?',
         'Seleccione la situación que mejor describa su empresa',
         1, -- SingleChoice
         1, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_1Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q4_1Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 1;
END

-- Answer options for Q4.1 with complete FODA/ODSR spectrum
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q4_1Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q4_1Id, 'Pre-revenue (sin ingresos)', 1, 'A', 'Alto riesgo financiero', 'S', 'Modo supervivencia', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_1Id, 'Primeros ingresos', 2, 'D', 'Flujo de caja débil', 'R', 'Requiere pivot financiero', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_1Id, 'Break-even (punto de equilibrio)', 3, 'O', 'Momento de inflexión', 'D', 'Consolidar posición', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_1Id, 'Rentable', 4, 'F', 'Generación de utilidades', 'O', 'Reinversión para crecimiento', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_1Id, 'Altamente rentable', 5, 'F', 'Flujo de caja sólido', 'O', 'Expansión agresiva posible', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_1Id, 'Con pérdidas operativas', 1, 'A', 'Situación crítica', 'S', 'Urgente reestructuración', 6, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 4.2: Fuentes de financiamiento (MultiChoice with FODA/ODSR)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinancieroId, @BlockFinancieroId,
         '¿Qué fuentes de financiamiento ha utilizado o planea utilizar?',
         'Seleccione todas las que apliquen',
         2, -- MultiChoice
         2, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_2Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q4_2Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 2;
END

-- Answer options for Q4.2 with varied FODA/ODSR patterns
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q4_2Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q4_2Id, 'Recursos propios', 3, 'F', 'Control total del negocio', 'D', 'Proteger capital propio', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Friends & Family', 2, 'D', 'Recursos limitados', 'S', 'Apoyo básico de supervivencia', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Angel investors', 4, 'O', 'Capital inteligente', 'O', 'Aceleración con mentores', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Venture Capital', 5, 'F', 'Recursos para escalar', 'O', 'Crecimiento exponencial', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Crowdfunding', 3, 'O', 'Validación de mercado', 'R', 'Nueva forma de financiamiento', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Préstamos bancarios', 2, 'A', 'Deuda con intereses', 'D', 'Gestión conservadora', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Subvenciones gubernamentales', 4, 'F', 'Capital no dilutivo', 'O', 'Apoyo institucional', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q4_2Id, 'Revenue-based financing', 3, 'O', 'Financiamiento flexible', 'D', 'Preservar equity', 8, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 4.3: Proyección de ingresos (Numeric)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicFinancieroId, @BlockFinancieroId,
         '¿Cuál es su proyección de ingresos para los próximos 12 meses (en USD)?',
         'Ingrese el monto proyectado en dólares',
         4, -- Numeric
         3, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q4_3Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q4_3Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockFinancieroId AND [Order] = 3;
END

-- =============================================
-- Create ProjectQuestions for Block 5: Innovación y Tecnología
-- IMPORTANT: IsUsedForDiagnosis = 1, AppliesToPhase = 2 (Both)
-- =============================================
DECLARE @Q5_1Id BIGINT, @Q5_2Id BIGINT, @Q5_3Id BIGINT, @Q5_4Id BIGINT;

-- Question 5.1: Nivel de innovación (SingleChoice with FODA/ODSR)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 1)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId, @BlockInnovacionId,
         '¿Cómo calificaría el nivel de innovación de su producto/servicio?',
         'Evalúe el grado de innovación comparado con el mercado',
         1, -- SingleChoice
         1, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_1Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q5_1Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 1;
END

-- Answer options for Q5.1 showcasing all FODA and ODSR types
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q5_1Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q5_1Id, 'Disruptivo - Cambia las reglas del juego', 5, 'F', 'Innovación revolucionaria', 'O', 'Crear nuevo mercado', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'Altamente innovador', 4, 'F', 'Ventaja tecnológica clara', 'O', 'Liderazgo de mercado', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'Innovación incremental', 3, 'O', 'Mejoras sobre lo existente', 'D', 'Defender diferenciación', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'Adaptación de tecnología existente', 2, 'D', 'Sin ventaja tecnológica propia', 'R', 'Buscar nuevo enfoque', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'Modelo tradicional mejorado', 2, 'A', 'Vulnerable a disrupciones', 'S', 'Mantener relevancia', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_1Id, 'Sin innovación significativa', 1, 'A', 'Alto riesgo de obsolescencia', 'R', 'Urgente transformación', 6, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 5.2: Tecnologías utilizadas (MultiChoice with FODA/ODSR)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 2)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId, @BlockInnovacionId,
         '¿Qué tecnologías emergentes está utilizando o planea utilizar?',
         'Seleccione todas las tecnologías relevantes',
         2, -- MultiChoice
         2, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_2Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q5_2Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 2;
END

-- Answer options for Q5.2 with comprehensive FODA/ODSR coverage
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q5_2Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q5_2Id, 'Inteligencia Artificial / Machine Learning', 5, 'F', 'Tecnología de vanguardia', 'O', 'Automatización y escalabilidad', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Blockchain', 4, 'O', 'Tecnología emergente prometedora', 'R', 'Nuevos modelos de negocio', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Internet de las Cosas (IoT)', 4, 'F', 'Conectividad y datos', 'O', 'Expansión de servicios', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Realidad Virtual/Aumentada', 3, 'O', 'Experiencias inmersivas', 'D', 'Nicho específico', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Big Data & Analytics', 4, 'F', 'Decisiones basadas en datos', 'O', 'Ventaja competitiva por insights', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Cloud Computing', 3, 'D', 'Tecnología commodity', 'D', 'Optimización de costos', 6, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Biotecnología', 5, 'F', 'Alta barrera de entrada', 'O', 'Impacto transformador', 7, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Robótica', 4, 'O', 'Automatización avanzada', 'S', 'Eficiencia operativa', 8, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Energías renovables', 4, 'O', 'Sustentabilidad y tendencia', 'R', 'Impacto ambiental positivo', 9, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_2Id, 'Ninguna tecnología emergente', 1, 'A', 'Riesgo de quedar obsoleto', 'S', 'Supervivencia básica', 10, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 5.3: Protección de propiedad intelectual (SingleChoice)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 3)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId, @BlockInnovacionId,
         '¿Qué tipo de protección de propiedad intelectual tiene o planea tener?',
         'Seleccione el nivel de protección legal',
         1, -- SingleChoice
         3, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_3Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q5_3Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 3;
END

-- Answer options for Q5.3 balancing all FODA/ODSR types
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectAnswerOptions] WHERE ProjectQuestionId = @Q5_3Id)
BEGIN
    INSERT INTO [businessincubators].[ProjectAnswerOptions]
        (ProjectQuestionId, Text, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, [Order],
         IsTextCustomized, IsScoreCustomized, IsFodaCustomized, IsFodaExplanationCustomized, IsOdsrCustomized, IsOdsrExplanationCustomized, IsOrderCustomized, IsFollowUpTextCustomized)
    VALUES
        (@Q5_3Id, 'Patentes otorgadas', 5, 'F', 'Máxima protección legal', 'D', 'Defender innovación', 1, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_3Id, 'Patentes en trámite', 4, 'O', 'Protección en proceso', 'O', 'Posición futura fuerte', 2, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_3Id, 'Secretos comerciales', 3, 'F', 'Conocimiento protegido', 'D', 'Mantener ventaja', 3, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_3Id, 'Marcas registradas', 3, 'O', 'Protección de marca', 'D', 'Defender identidad', 4, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_3Id, 'Derechos de autor', 2, 'D', 'Protección básica', 'S', 'Protección mínima', 5, 0, 0, 0, 0, 0, 0, 0, 0),
        (@Q5_3Id, 'Sin protección formal', 1, 'A', 'Vulnerable a copias', 'R', 'Urgente protección legal', 6, 0, 0, 0, 0, 0, 0, 0, 0);
END

-- Question 5.4: Plan de I+D (FreeText)
IF NOT EXISTS (SELECT 1 FROM [businessincubators].[ProjectQuestions] WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 4)
BEGIN
    INSERT INTO [businessincubators].[ProjectQuestions]
        (ProjectTopicId, ProjectBlockId, Text, HelpText, AnswerType, [Order], IsRequired, IsUsedForDiagnosis, AppliesToPhase, IsUsedForMentoringPlan,
         IsTextCustomized, IsAnswerTypeCustomized, IsAppliesToPhaseCustomized, IsMentoringPlanCustomized, IsDiagnosisCustomized,
         IsOrderCustomized, IsHelpTextCustomized, IsRequiredCustomized, IsAnswerOptionsCustomized)
    VALUES
        (@TopicInnovacionId, @BlockInnovacionId,
         'Describa su plan de investigación y desarrollo para los próximos 12 meses',
         'Detalle las iniciativas de innovación planificadas',
         3, -- FreeText
         4, 1,
         1, -- IsUsedForDiagnosis = 1
         2, -- AppliesToPhase = Both
         1, -- IsUsedForMentoringPlan
         0, 0, 0, 0, 0, 0, 0, 0, 0);
    
    SET @Q5_4Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Q5_4Id = Id FROM [businessincubators].[ProjectQuestions] 
    WHERE ProjectBlockId = @BlockInnovacionId AND [Order] = 4;
END

-- =============================================
-- Summary and Verification
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'PROJECT KNOWLEDGE STRUCTURE SEED COMPLETE';
PRINT '========================================';
PRINT '';

-- Verify structure creation
DECLARE @ModuleCount INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectModules] WHERE ProjectKnowledgeStructureId = @KnowledgeStructureId);
PRINT '✓ ProjectModules created: ' + CAST(@ModuleCount AS NVARCHAR(10)) + ' modules';

DECLARE @TopicCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectTopics] t
    INNER JOIN [businessincubators].[ProjectModules] m ON t.ProjectModuleId = m.Id
    WHERE m.ProjectKnowledgeStructureId = @KnowledgeStructureId
);
PRINT '✓ ProjectTopics created: ' + CAST(@TopicCount AS NVARCHAR(10)) + ' topics';

DECLARE @BlockCount INT = (SELECT COUNT(*) FROM [businessincubators].[ProjectBlocks] WHERE ProjectId = @DemoProjectId);
PRINT '✓ ProjectBlocks created: ' + CAST(@BlockCount AS NVARCHAR(10)) + ' blocks';

-- Verify questions
DECLARE @QuestionCount INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId
);
PRINT '✓ ProjectQuestions created: ' + CAST(@QuestionCount AS NVARCHAR(10)) + ' questions';

-- Verify diagnosis configuration
DECLARE @DiagnosisQuestionsInfoGeneral INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId 
    AND b.Name = 'Información General'
    AND q.IsUsedForDiagnosis = 0
);
PRINT '✓ Questions in "Información General" with IsUsedForDiagnosis=0: ' + CAST(@DiagnosisQuestionsInfoGeneral AS NVARCHAR(10));

DECLARE @DiagnosisQuestionsOthers INT = (
    SELECT COUNT(*) 
    FROM [businessincubators].[ProjectQuestions] q
    INNER JOIN [businessincubators].[ProjectBlocks] b ON q.ProjectBlockId = b.Id
    WHERE b.ProjectId = @DemoProjectId 
    AND b.Name != 'Información General'
    AND q.IsUsedForDiagnosis = 1
);
PRINT '✓ Questions in other blocks with IsUsedForDiagnosis=1: ' + CAST(@DiagnosisQuestionsOthers AS NVARCHAR(10));

-- Verify FODA coverage
DECLARE @FodaF INT = (SELECT COUNT(DISTINCT ProjectQuestionId) FROM [businessincubators].[ProjectAnswerOptions] WHERE Foda = 'F');
DECLARE @FodaO INT = (SELECT COUNT(DISTINCT ProjectQuestionId) FROM [businessincubators].[ProjectAnswerOptions] WHERE Foda = 'O');
DECLARE @FodaD INT = (SELECT COUNT(DISTINCT ProjectQuestionId) FROM [businessincubators].[ProjectAnswerOptions] WHERE Foda = 'D');
DECLARE @FodaA INT = (SELECT COUNT(DISTINCT ProjectQuestionId) FROM [businessincubators].[ProjectAnswerOptions] WHERE Foda = 'A');
PRINT '';
PRINT 'FODA Type Coverage:';
PRINT '  - Fortalezas (F): ' + CAST(@FodaF AS NVARCHAR(10)) + ' questions';
PRINT '  - Oportunidades (O): ' + CAST(@FodaO AS NVARCHAR(10)) + ' questions';
PRINT '  - Debilidades (D): ' + CAST(@FodaD AS NVARCHAR(10)) + ' questions';
PRINT '  - Amenazas (A): ' + CAST(@FodaA AS NVARCHAR(10)) + ' questions';

-- Verify ODSR coverage
DECLARE @OdsrO INT = (SELECT COUNT(DISTINCT ProjectQuestionId) FROM [businessincubators].[ProjectAnswerOptions] WHERE Odsr = 'O');
DECLARE @OdsrD INT = (SELECT COUNT(DISTINCT ProjectQuestionId) FROM [businessincubators].[ProjectAnswerOptions] WHERE Odsr = 'D');
DECLARE @OdsrS INT = (SELECT COUNT(DISTINCT ProjectQuestionId) FROM [businessincubators].[ProjectAnswerOptions] WHERE Odsr = 'S');
DECLARE @OdsrR INT = (SELECT COUNT(DISTINCT ProjectQuestionId) FROM [businessincubators].[ProjectAnswerOptions] WHERE Odsr = 'R');
PRINT '';
PRINT 'ODSR Type Coverage:';
PRINT '  - Ofensiva (O): ' + CAST(@OdsrO AS NVARCHAR(10)) + ' questions';
PRINT '  - Defensiva (D): ' + CAST(@OdsrD AS NVARCHAR(10)) + ' questions';
PRINT '  - Supervivencia (S): ' + CAST(@OdsrS AS NVARCHAR(10)) + ' questions';
PRINT '  - Reorientación (R): ' + CAST(@OdsrR AS NVARCHAR(10)) + ' questions';

PRINT '';
PRINT '========================================';
PRINT 'SEED DATA ENRICHMENT COMPLETE';
PRINT '========================================';

-- End batch to avoid variable name conflicts with subsequent scripts
GO