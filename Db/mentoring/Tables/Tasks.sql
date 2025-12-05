CREATE TABLE [mentoring].[Tasks]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [QuestionId] BIGINT NOT NULL,
    [ForAnswer] TINYINT NOT NULL, -- 1 = Yes, 2 = No, 3 = Partially
    [Description] NVARCHAR(MAX) NOT NULL,
    [Status] TINYINT NOT NULL DEFAULT 0, -- 0 = NotStarted, 1 = InProgress, 2 = Completed

    CONSTRAINT FK_MentoringTasks_MentoringQuestions FOREIGN KEY ([QuestionId])
        REFERENCES [mentoring].[Questions] ([Id])
        ON DELETE CASCADE
);
