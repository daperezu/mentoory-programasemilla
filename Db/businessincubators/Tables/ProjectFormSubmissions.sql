CREATE TABLE [businessincubators].[ProjectFormSubmissions] (
    [Id]                 BIGINT IDENTITY (1, 1) NOT NULL,
    [ExternalId]         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [ProjectId]          BIGINT NOT NULL,
    [ParticipantUserId]  NVARCHAR(450) NOT NULL,
    [FormId]             BIGINT NOT NULL,
    [Status]             INT NOT NULL DEFAULT 1, -- 1=Draft, 2=Submitted, 3=Approved, 4=Rejected
    [DraftData]          NVARCHAR(MAX) NULL,
    [StartedAt]          DATETIME2 NOT NULL,
    [SubmittedAt]        DATETIME2 NULL,
    [ApprovedAt]         DATETIME2 NULL,
    [ApprovedByUserId]   NVARCHAR(450) NULL,
    [RejectionReason]    NVARCHAR(500) NULL,
    [RejectedAt]         DATETIME2 NULL,
    [FormSchemaVersion]  INT NOT NULL DEFAULT 1,
    [Phase]              INT NOT NULL DEFAULT 1, -- 1=Start, 2=Final, 4=Undefined
    [ProjectStageId]     BIGINT NULL,
    [CompletionPercentage] INT NOT NULL DEFAULT 0,
    [LastAutoSaveAt]     DATETIME2 NULL,
    [TotalQuestions]     INT NOT NULL DEFAULT 0,
    [AnsweredQuestions]  INT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_ProjectFormSubmissions] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ProjectFormSubmissions_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [businessincubators].[Projects] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProjectFormSubmissions_ProjectStages] FOREIGN KEY ([ProjectStageId]) REFERENCES [businessincubators].[ProjectStages] ([Id])
    -- Note: Cross-schema foreign keys are not included to maintain modular boundaries
    -- - FormId references diagnostics.Forms (validated at application layer)
    -- - ParticipantUserId and ApprovedByUserId reference auth/identity system (validated at application layer)
);