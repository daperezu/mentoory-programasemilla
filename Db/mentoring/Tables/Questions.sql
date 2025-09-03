CREATE TABLE [mentoring].[Questions]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SourceQuestionId] BIGINT NULL,
    [TopicId] BIGINT NOT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT FK_Questions_Topics FOREIGN KEY ([TopicId])
        REFERENCES [mentoring].[Topics] ([Id])
        ON DELETE CASCADE
);
