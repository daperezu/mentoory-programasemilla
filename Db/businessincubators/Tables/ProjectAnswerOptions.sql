CREATE TABLE [businessincubators].[ProjectAnswerOptions]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ProjectQuestionId] BIGINT NOT NULL,
    [SourceAnswerOptionId] BIGINT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    [IsTextCustomized] BIT NOT NULL DEFAULT 0,
    [Score] INT NOT NULL,
    [IsScoreCustomized] BIT NOT NULL DEFAULT 0,
    [Foda] CHAR(1) NOT NULL,
    [IsFodaCustomized] BIT NOT NULL DEFAULT 0,
    [FodaExplanation] NVARCHAR(MAX) NOT NULL,
    [IsFodaExplanationCustomized] BIT NOT NULL DEFAULT 0,
    [Odsr] CHAR(1) NOT NULL,
    [IsOdsrCustomized] BIT NOT NULL DEFAULT 0,
    [OdsrExplanation] NVARCHAR(MAX) NOT NULL,
    [IsOdsrExplanationCustomized] BIT NOT NULL DEFAULT 0,
    [Order] INT NOT NULL,
    [IsOrderCustomized] BIT NOT NULL DEFAULT 0,
    [FollowUpQuestionText] NVARCHAR(MAX) NULL,
    [IsFollowUpTextCustomized] BIT NOT NULL DEFAULT 0,

    FOREIGN KEY ([ProjectQuestionId]) REFERENCES [businessincubators].[ProjectQuestions]([Id]) ON DELETE CASCADE
);
