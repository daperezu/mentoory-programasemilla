CREATE TABLE [diagnostics].[DiagnosisPhaseSummaries] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserProjectDiagnosisId] bigint NOT NULL,
    [Phase] int NOT NULL,
    [CompletedAt] datetime2(7) NOT NULL,
    [AnswerCount] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_DiagnosisPhaseSummaries] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DiagnosisPhaseSummaries_UserProjectDiagnoses] FOREIGN KEY ([UserProjectDiagnosisId]) REFERENCES [diagnostics].[UserProjectDiagnoses]([Id]) ON DELETE CASCADE
);
GO