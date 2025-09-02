CREATE TABLE [businessincubators].[ProjectQuestions]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ProjectTopicId] BIGINT NULL,
    [ProjectBlockId] BIGINT NOT NULL,
    [SourceQuestionId] BIGINT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    [IsTextCustomized] BIT NOT NULL DEFAULT 0,
    [AnswerType] INT NOT NULL,
    [IsAnswerTypeCustomized] BIT NOT NULL DEFAULT 0,
    [AppliesToPhase] INT NOT NULL,
    [IsAppliesToPhaseCustomized] BIT NOT NULL DEFAULT 0,
    [IsUsedForMentoringPlan] BIT NOT NULL,
    [IsMentoringPlanCustomized] BIT NOT NULL DEFAULT 0,
    [IsUsedForDiagnosis] BIT NOT NULL,
    [IsDiagnosisCustomized] BIT NOT NULL DEFAULT 0,
    [Order] INT NOT NULL,
    [IsOrderCustomized] BIT NOT NULL DEFAULT 0,
    [HelpText] NVARCHAR(MAX) NULL,
    [IsHelpTextCustomized] BIT NOT NULL DEFAULT 0,
    [IsRequired] BIT NOT NULL DEFAULT 1,
    [IsRequiredCustomized] BIT NOT NULL DEFAULT 0,
    [IsAnswerOptionsCustomized] BIT NOT NULL DEFAULT 0,
    [LastSyncedAt] DATETIME2 NULL,

    CONSTRAINT [FK_ProjectQuestions_ProjectTopics] FOREIGN KEY ([ProjectTopicId]) REFERENCES [businessincubators].[ProjectTopics]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProjectQuestions_ProjectBlocks] FOREIGN KEY ([ProjectBlockId]) REFERENCES [businessincubators].[ProjectBlocks]([Id]) ON DELETE CASCADE
);
