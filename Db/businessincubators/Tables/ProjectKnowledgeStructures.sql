CREATE TABLE [businessincubators].[ProjectKnowledgeStructures]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [SourceKnowledgeStructureId] BIGINT NULL,
    [ProjectId] BIGINT NOT NULL UNIQUE,
    [Name] NVARCHAR(200) NOT NULL,
    [IsNameCustomized] BIT NOT NULL DEFAULT 0,
    [Description] NVARCHAR(1000) NULL,
    [IsDescriptionCustomized] BIT NOT NULL DEFAULT 0,
    [IsLocked] BIT NOT NULL CONSTRAINT DF_ProjectKnowledgeStructures_IsLocked DEFAULT 0,
    [LockedAt] DATETIME2 NULL,
    [LockedReason] NVARCHAR(250) NULL,
    [CurrentVersion] INT NOT NULL DEFAULT 1,
    [LastSyncedAt] DATETIME2 NULL,

    FOREIGN KEY ([ProjectId]) REFERENCES [businessincubators].[Projects]([Id]) ON DELETE CASCADE
);