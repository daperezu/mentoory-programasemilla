CREATE TABLE [knowledgestructure].[SubjectResources]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [SubjectId] BIGINT NOT NULL,
    [Title] NVARCHAR(300) NOT NULL,
    [Url] NVARCHAR(1000) NOT NULL,
    [Type] NVARCHAR(100) NOT NULL,
    [EstimatedMinutes] INT NULL,
    [Order] INT NOT NULL,

    CONSTRAINT [FK_SubjectResources_Subjects]
        FOREIGN KEY ([SubjectId]) REFERENCES [knowledgestructure].[Subjects]([Id])
        ON DELETE CASCADE
);
