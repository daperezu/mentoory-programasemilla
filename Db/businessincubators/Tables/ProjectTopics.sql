CREATE TABLE [businessincubators].[ProjectTopics]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ProjectModuleId] BIGINT NOT NULL,
    [SourceTopicId] BIGINT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [IsNameCustomized] BIT NOT NULL DEFAULT 0,
    [Order] INT NOT NULL,
    [IsOrderCustomized] BIT NOT NULL DEFAULT 0,
    [LastSyncedAt] DATETIME2 NULL,

    FOREIGN KEY ([ProjectModuleId]) REFERENCES [businessincubators].[ProjectModules]([Id]) ON DELETE CASCADE
);
