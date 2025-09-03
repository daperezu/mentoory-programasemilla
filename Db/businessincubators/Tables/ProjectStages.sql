CREATE TABLE [businessincubators].[ProjectStages] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [Type] INT NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(2000) NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(100) NULL,
    CONSTRAINT [PK_ProjectStages] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ProjectStages_Projects] FOREIGN KEY ([ProjectId]) 
        REFERENCES [businessincubators].[Projects]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ProjectStages_ProjectId_Type] UNIQUE ([ProjectId], [Type]),
    CONSTRAINT [CHK_ProjectStages_Dates] CHECK ([StartDate] < [EndDate])
);
GO

CREATE INDEX [IX_ProjectStages_ProjectId] ON [businessincubators].[ProjectStages]([ProjectId]);
GO

CREATE INDEX [IX_ProjectStages_Type] ON [businessincubators].[ProjectStages]([Type]);
GO

CREATE INDEX [IX_ProjectStages_Dates] ON [businessincubators].[ProjectStages]([StartDate], [EndDate]);
GO