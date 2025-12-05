CREATE TABLE [businessincubators].[ProjectSubjectAnswerOptions]
(
    [ProjectSubjectId] BIGINT NOT NULL,
    [ProjectAnswerOptionId] BIGINT NOT NULL,

    CONSTRAINT [PK_ProjectSubjectAnswerOptions] PRIMARY KEY ([ProjectSubjectId], [ProjectAnswerOptionId]),
    FOREIGN KEY ([ProjectSubjectId]) REFERENCES [businessincubators].[ProjectSubjects]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([ProjectAnswerOptionId]) REFERENCES [businessincubators].[ProjectAnswerOptions]([Id]) ON DELETE NO ACTION
);