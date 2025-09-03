CREATE TABLE [knowledgestructure].[KnowledgeStructureModules]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [KnowledgeStructureId] BIGINT NOT NULL,
    [ModuleId] BIGINT NOT NULL,
    [Order] INT NOT NULL,

    FOREIGN KEY ([KnowledgeStructureId]) REFERENCES [knowledgestructure].[KnowledgeStructures]([Id]),
    FOREIGN KEY ([ModuleId]) REFERENCES [knowledgestructure].[Modules]([Id])
);
