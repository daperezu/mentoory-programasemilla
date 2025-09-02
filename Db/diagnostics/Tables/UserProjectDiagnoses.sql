CREATE TABLE [diagnostics].[UserProjectDiagnoses] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [ProjectId] bigint NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [Status] int NOT NULL DEFAULT 1,
    [CreatedAt] datetime2(7) NOT NULL,
    [LastUpdatedAt] datetime2(7) NULL,
    CONSTRAINT [PK_UserProjectDiagnoses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_UserProjectDiagnoses_Project_User] UNIQUE NONCLUSTERED ([ProjectId] ASC, [UserId] ASC)
);
GO