CREATE TABLE [knowledgestructure].[Topics]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NULL
);
