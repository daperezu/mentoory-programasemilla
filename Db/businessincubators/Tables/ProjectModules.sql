CREATE TABLE [businessincubators].[ProjectModules]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ProjectKnowledgeStructureId] BIGINT NOT NULL,
    [SourceModuleId] BIGINT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [IsNameCustomized] BIT NOT NULL DEFAULT 0,
    [Order] INT NOT NULL,
    [IsOrderCustomized] BIT NOT NULL DEFAULT 0,
    [LastSyncedAt] DATETIME2 NULL,

    FOREIGN KEY ([ProjectKnowledgeStructureId]) REFERENCES [businessincubators].[ProjectKnowledgeStructures]([Id]) ON DELETE CASCADE
);