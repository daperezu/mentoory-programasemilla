CREATE TABLE [diagnostics].[DiagnosisAnswers] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserProjectDiagnosisId] BIGINT NULL,
    [ProjectId] BIGINT NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [ModuleId] BIGINT NULL,
    [ModuleName] NVARCHAR(200) NULL,
    [TopicId] BIGINT NULL,
    [TopicName] NVARCHAR(200) NULL,
    [BlockId] BIGINT NOT NULL,
    [BlockName] NVARCHAR(200) NOT NULL,
    [QuestionId] BIGINT NOT NULL,
    [QuestionText] NVARCHAR(MAX) NOT NULL,
    [AnswerOptionId] BIGINT NOT NULL,
    [AnswerOptionText] NVARCHAR(MAX) NOT NULL,
    [AnswerOptionUserInput] NVARCHAR(MAX) NULL,
    [FollowUpQuestionText] NVARCHAR(MAX) NULL,
    [FollowUpAnswerUserInput] NVARCHAR(MAX) NULL,
    [Score] INT NOT NULL,
    [Foda] CHAR(1) NOT NULL,
    [FodaExplanation] NVARCHAR(MAX) NOT NULL,
    [Odsr] CHAR(1) NOT NULL,
    [OdsrExplanation] NVARCHAR(MAX) NOT NULL,
    [Phase] INT NOT NULL,
    [IsUsedForMentoringPlan] BIT NOT NULL,
    [IsUsedForDiagnosis] BIT NOT NULL,
    [Order] INT NOT NULL,
    [SubmittedAt] DATETIME2(7) NOT NULL,
    CONSTRAINT [PK_DiagnosisAnswers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DiagnosisAnswers_UserProjectDiagnoses] FOREIGN KEY ([UserProjectDiagnosisId]) REFERENCES [diagnostics].[UserProjectDiagnoses]([Id])
);
GO

-- Create unique constraint to prevent duplicate answers within a project
CREATE UNIQUE NONCLUSTERED INDEX [UQ_DiagnosisAnswers_ProjectId_UserId_QuestionId_Phase] 
ON [diagnostics].[DiagnosisAnswers]([ProjectId] ASC, [UserId] ASC, [QuestionId] ASC, [Phase] ASC);
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_DiagnosisAnswers_ProjectId_UserId] 
ON [diagnostics].[DiagnosisAnswers]([ProjectId] ASC, [UserId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_DiagnosisAnswers_QuestionId] 
ON [diagnostics].[DiagnosisAnswers]([QuestionId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_DiagnosisAnswers_Phase] 
ON [diagnostics].[DiagnosisAnswers]([Phase] ASC);
GO
