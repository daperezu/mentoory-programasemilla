CREATE TABLE [mentoring].[Sessions]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProgramId] BIGINT NOT NULL,
    [ScheduledAt] DATETIME2 NOT NULL,
    [Notes] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT FK_MentoringSessions_MentoringPrograms FOREIGN KEY ([ProgramId])
        REFERENCES [mentoring].[Programs] ([Id])
        ON DELETE CASCADE
);
