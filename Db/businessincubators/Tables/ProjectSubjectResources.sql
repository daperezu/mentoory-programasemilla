CREATE TABLE [businessincubators].[ProjectSubjectResources]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ProjectSubjectId] BIGINT NOT NULL,
    [SourceSubjectResourceId] BIGINT NULL,
    [Title] NVARCHAR(300) NOT NULL,
    [IsTitleCustomized] BIT NOT NULL DEFAULT 0,
    [Url] NVARCHAR(1000) NOT NULL,
    [IsUrlCustomized] BIT NOT NULL DEFAULT 0,
    [Type] NVARCHAR(100) NOT NULL,
    [IsTypeCustomized] BIT NOT NULL DEFAULT 0,
    [EstimatedMinutes] INT NULL,
    [IsEstimatedMinutesCustomized] BIT NOT NULL DEFAULT 0,
    [Order] INT NOT NULL,
    [IsOrderCustomized] BIT NOT NULL DEFAULT 0,

    FOREIGN KEY ([ProjectSubjectId]) REFERENCES [businessincubators].[ProjectSubjects]([Id]) ON DELETE CASCADE
);
