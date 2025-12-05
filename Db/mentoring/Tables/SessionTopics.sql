CREATE TABLE [mentoring].[SessionTopics]
(
    [SessionId] BIGINT NOT NULL,
    [TopicId] BIGINT NOT NULL,

    CONSTRAINT PK_MentoringSessionTopics PRIMARY KEY ([SessionId], [TopicId]),

    CONSTRAINT FK_MentoringSessionTopics_MentoringSessions FOREIGN KEY ([SessionId])
        REFERENCES [mentoring].[Sessions] ([Id])
        ON DELETE CASCADE,

    CONSTRAINT FK_MentoringSessionTopics_MentoringTopics FOREIGN KEY ([TopicId])
        REFERENCES [mentoring].[Topics] ([Id])
        ON DELETE CASCADE
);
