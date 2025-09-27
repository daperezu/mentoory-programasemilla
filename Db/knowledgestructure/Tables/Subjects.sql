CREATE TABLE [knowledgestructure].[Subjects]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Title] NVARCHAR(300) NOT NULL,
    [Content] NVARCHAR(MAX) NULL
);
