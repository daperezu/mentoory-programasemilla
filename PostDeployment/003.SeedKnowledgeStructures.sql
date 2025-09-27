-- ==========================================================================================
-- Post-Deployment Script for Seeding default Packages and Limits for the Subscription Module
-- ==========================================================================================

; -- Sepparator semicolon before WITH statement

MERGE knowledgestructure.KnowledgeStructures AS target
USING (SELECT N'Estructura de Conocimiento Base' AS Name) AS source
ON target.Name = source.Name
WHEN NOT MATCHED THEN INSERT (Name) VALUES (source.Name);

DECLARE @KnowledgeStructureId BIGINT =
    (SELECT Id FROM knowledgestructure.KnowledgeStructures WHERE Name = N'Estructura de Conocimiento Base');

-- Declare mapping table
DECLARE @Seed TABLE (
    Module NVARCHAR(200),
    Topic NVARCHAR(200),
    ModuleOrder INT
);

-- Add all modules/topics
INSERT INTO @Seed (Module, Topic, ModuleOrder)
VALUES
(N'Habilidades para emprender', N'Esencia corporativa', 1),
(N'Habilidades para emprender', N'Alianzas estratégicas', 1),
(N'Creatividad innovación', N'Innovación', 2),
(N'Mercado', N'Clientela potencia', 3),
(N'Mercado', N'Mercado', 3),
(N'Mercado', N'Validación', 3),
(N'Mercado', N'Propuesta de valor', 3),
(N'Costos financiero', N'Comercio electrónico', 4),
(N'Costos financiero', N'Cadenas de valor', 4),
(N'Costos financiero', N'Desarrollo financiero', 4),
(N'Costos financiero', N'Producción y costos', 4),
(N'Imágen y comunicación', N'Análisis de mercado', 5),
(N'Imágen y comunicación', N'Análisis tributario', 5),
(N'Imágen y comunicación', N'Ventas al estado / Sicop', 5),
(N'Imágen y comunicación', N'Ventas', 5),
(N'Imágen y comunicación', N'Digital / TIC''s', 5),
(N'Comunicación efectiva', N'Pitch', 6),
(N'Comunicación efectiva', N'Marca', 6),
(N'Formalización', N'Formalización', 7),
(N'Formalización', N'RRHH', 7);

-- Insert all Modules
MERGE knowledgestructure.Modules AS target
USING (SELECT DISTINCT Module FROM @Seed) AS source(Name)
ON target.Name = source.Name
WHEN NOT MATCHED THEN INSERT (Name) VALUES (source.Name);

-- Insert all Topics
MERGE knowledgestructure.Topics AS target
USING (SELECT DISTINCT Topic FROM @Seed) AS source(Name)
ON target.Name = source.Name
WHEN NOT MATCHED THEN INSERT (Name) VALUES (source.Name);

-- Link Modules to KnowledgeStructure
MERGE knowledgestructure.KnowledgeStructureModules AS target
USING (
    SELECT DISTINCT
        @KnowledgeStructureId AS KnowledgeStructureId,
        m.Id AS ModuleId,
        MIN(ModuleOrder) AS [Order]
    FROM @Seed s
    JOIN knowledgestructure.Modules m ON m.Name = s.Module
    GROUP BY m.Id
) AS source
ON target.KnowledgeStructureId = source.KnowledgeStructureId AND target.ModuleId = source.ModuleId
WHEN NOT MATCHED THEN
    INSERT (KnowledgeStructureId, ModuleId, [Order])
    VALUES (source.KnowledgeStructureId, source.ModuleId, source.[Order]);

-- Link Topics to KnowledgeStructureModules
MERGE knowledgestructure.KnowledgeStructureTopics AS target
USING (
    SELECT
        ksm.Id AS KnowledgeStructureModuleId,
        t.Id AS TopicId,
        ROW_NUMBER() OVER (PARTITION BY ksm.Id ORDER BY s.Topic) AS [Order]
    FROM @Seed s
    JOIN knowledgestructure.Modules m ON m.Name = s.Module
    JOIN knowledgestructure.Topics t ON t.Name = s.Topic
    JOIN knowledgestructure.KnowledgeStructureModules ksm
        ON ksm.ModuleId = m.Id AND ksm.KnowledgeStructureId = @KnowledgeStructureId
) AS source
ON target.KnowledgeStructureModuleId = source.KnowledgeStructureModuleId AND target.TopicId = source.TopicId
WHEN NOT MATCHED THEN
    INSERT (KnowledgeStructureModuleId, TopicId, [Order])
    VALUES (source.KnowledgeStructureModuleId, source.TopicId, source.[Order]);

-- Insert Subjects for each Topic
MERGE knowledgestructure.Subjects AS target
USING (
    SELECT DISTINCT
        CONCAT(N'Introducción a ', s.Topic) AS Title,
        NULL AS Content
    FROM @Seed s
) AS source
ON target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (Title, Content) VALUES (source.Title, source.Content);

-- Link Subjects to KnowledgeStructureTopics
MERGE knowledgestructure.KnowledgeStructureSubjects AS target
USING (
    SELECT
        kst.Id AS KnowledgeStructureTopicId,
        sub.Id AS SubjectId,
        1 AS [Order]
    FROM @Seed s
    JOIN knowledgestructure.Topics t ON t.Name = s.Topic
    JOIN knowledgestructure.Subjects sub ON sub.Title = CONCAT(N'Introducción a ', s.Topic)
    JOIN knowledgestructure.Modules m ON m.Name = s.Module
    JOIN knowledgestructure.KnowledgeStructureModules ksm ON ksm.ModuleId = m.Id AND ksm.KnowledgeStructureId = @KnowledgeStructureId
    JOIN knowledgestructure.KnowledgeStructureTopics kst ON kst.TopicId = t.Id AND kst.KnowledgeStructureModuleId = ksm.Id
) AS source
ON target.KnowledgeStructureTopicId = source.KnowledgeStructureTopicId AND target.SubjectId = source.SubjectId
WHEN NOT MATCHED THEN
    INSERT (KnowledgeStructureTopicId, SubjectId, [Order])
    VALUES (source.KnowledgeStructureTopicId, source.SubjectId, source.[Order]);

-- Insert SubjectResources
MERGE knowledgestructure.SubjectResources AS target
USING (
    SELECT
        s.Id AS SubjectId,
        CONCAT(N'Recurso base para ', topic.Name) AS Title,
        N'' AS Url,
        N'1' AS [Type],
        30 AS EstimatedMinutes,
        1 AS [Order]
    FROM knowledgestructure.Subjects s
    JOIN @Seed seed ON s.Title = CONCAT(N'Introducción a ', seed.Topic)
    JOIN knowledgestructure.Topics topic ON topic.Name = seed.Topic
) AS source
ON target.SubjectId = source.SubjectId AND target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (SubjectId, Title, Url, [Type], EstimatedMinutes, [Order])
    VALUES (source.SubjectId, source.Title, source.Url, source.Type, source.EstimatedMinutes, source.[Order]);