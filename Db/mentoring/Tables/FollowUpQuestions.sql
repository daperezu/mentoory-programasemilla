CREATE TABLE [mentoring].[FollowUpQuestions]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [QuestionId] BIGINT NOT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT FK_FollowUpQuestions_Questions FOREIGN KEY ([QuestionId])
        REFERENCES [mentoring].Questions ([Id])
        ON DELETE CASCADE
);
