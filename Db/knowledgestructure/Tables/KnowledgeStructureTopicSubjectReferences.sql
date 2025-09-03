CREATE TABLE [knowledgestructure].[KnowledgeStructureTopicSubjectReferences]
(
    [KnowledgeStructureTopicId] BIGINT NOT NULL,
    [SubjectId] BIGINT NOT NULL,
    [Order] INT NOT NULL,
    CONSTRAINT [PK_KnowledgeStructureTopicSubjectReferences] PRIMARY KEY CLUSTERED ([KnowledgeStructureTopicId], [SubjectId]),
    CONSTRAINT [FK_KnowledgeStructureTopicSubjectReferences_KnowledgeStructureTopics] FOREIGN KEY ([KnowledgeStructureTopicId]) REFERENCES [knowledgestructure].[KnowledgeStructureTopics] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_KnowledgeStructureTopicSubjectReferences_Subjects] FOREIGN KEY ([SubjectId]) REFERENCES [knowledgestructure].[Subjects] ([Id])
);
GO

-- Add indexes for performance
CREATE NONCLUSTERED INDEX [IX_KnowledgeStructureTopicSubjectReferences_SubjectId] ON [knowledgestructure].[KnowledgeStructureTopicSubjectReferences] ([SubjectId]);
GO