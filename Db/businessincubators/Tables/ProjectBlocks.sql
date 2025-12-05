CREATE TABLE [businessincubators].[ProjectBlocks]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [SourceBlockId] BIGINT NULL,
    [ProjectId] BIGINT NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [IsNameCustomized] BIT NOT NULL DEFAULT 0,

    CONSTRAINT [FK_ProjectBlocks_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [businessincubators].[Projects]([Id]) ON DELETE CASCADE
);
