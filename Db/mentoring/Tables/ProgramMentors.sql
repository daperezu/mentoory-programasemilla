CREATE TABLE [mentoring].[ProgramMentors]
(
    [ProgramId] BIGINT NOT NULL,
    [MentorUserId] NVARCHAR(450) NOT NULL,

    CONSTRAINT PK_MentoringProgramMentors PRIMARY KEY ([ProgramId], [MentorUserId]),

    CONSTRAINT FK_MentoringProgramMentors_Programs FOREIGN KEY ([ProgramId])
        REFERENCES [mentoring].[Programs] ([Id])
        ON DELETE CASCADE
);
