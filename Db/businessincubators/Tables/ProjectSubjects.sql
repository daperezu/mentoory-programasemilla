CREATE TABLE [businessincubators].[ProjectSubjects]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ProjectTopicId] BIGINT NOT NULL,
    [SourceSubjectId] BIGINT NULL,
    [Title] NVARCHAR(300) NOT NULL,
    [IsTitleCustomized] BIT NOT NULL DEFAULT 0,
    [Content] NVARCHAR(MAX) NULL,
    [IsContentCustomized] BIT NOT NULL DEFAULT 0,
    [Order] INT NOT NULL,
    [IsOrderCustomized] BIT NOT NULL DEFAULT 0,
    [LastSyncedAt] DATETIME2 NULL,

    FOREIGN KEY ([ProjectTopicId]) REFERENCES [businessincubators].[ProjectTopics]([Id]) ON DELETE CASCADE
);
