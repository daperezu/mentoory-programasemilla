CREATE TABLE [knowledgestructure].[KnowledgeStructureSubjects]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [KnowledgeStructureTopicId] BIGINT NOT NULL,
    [SubjectId] BIGINT NOT NULL,
    [Order] INT NOT NULL,

    FOREIGN KEY ([KnowledgeStructureTopicId]) REFERENCES [knowledgestructure].[KnowledgeStructureTopics]([Id]),
    FOREIGN KEY ([SubjectId]) REFERENCES [knowledgestructure].[Subjects]([Id])
);
