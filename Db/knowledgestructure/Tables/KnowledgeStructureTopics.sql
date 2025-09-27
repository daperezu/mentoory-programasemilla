CREATE TABLE [knowledgestructure].[KnowledgeStructureTopics]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [KnowledgeStructureModuleId] BIGINT NOT NULL,
    [TopicId] BIGINT NOT NULL,
    [Order] INT NOT NULL,

    FOREIGN KEY ([KnowledgeStructureModuleId]) REFERENCES [knowledgestructure].[KnowledgeStructureModules]([Id]),
    FOREIGN KEY ([TopicId]) REFERENCES [knowledgestructure].[Topics]([Id])
);
