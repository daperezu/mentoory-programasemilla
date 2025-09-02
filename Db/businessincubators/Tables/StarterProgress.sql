CREATE TABLE [businessincubators].[StarterProgress] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [CurrentPhase] NVARCHAR(50) NOT NULL, -- 'diagnosis', 'development', 'validation', 'implementation', 'growth'
    [PhaseStartDate] DATETIME2(7) NOT NULL,
    [PhaseExpectedEndDate] DATETIME2(7) NULL,
    [OverallProgress] DECIMAL(5,2) NOT NULL CONSTRAINT [DF_StarterProgress_OverallProgress] DEFAULT (0), -- 0.00 to 100.00
    [PhaseProgress] DECIMAL(5,2) NOT NULL CONSTRAINT [DF_StarterProgress_PhaseProgress] DEFAULT (0),
    [TasksCompleted] INT NOT NULL CONSTRAINT [DF_StarterProgress_TasksCompleted] DEFAULT (0),
    [TasksTotal] INT NOT NULL CONSTRAINT [DF_StarterProgress_TasksTotal] DEFAULT (0),
    [TasksOverdue] INT NOT NULL CONSTRAINT [DF_StarterProgress_TasksOverdue] DEFAULT (0),
    [FormsCompleted] INT NOT NULL CONSTRAINT [DF_StarterProgress_FormsCompleted] DEFAULT (0),
    [FormsTotal] INT NOT NULL CONSTRAINT [DF_StarterProgress_FormsTotal] DEFAULT (0),
    [FormsRejected] INT NOT NULL CONSTRAINT [DF_StarterProgress_FormsRejected] DEFAULT (0),
    [MilestonesAchieved] INT NOT NULL CONSTRAINT [DF_StarterProgress_MilestonesAchieved] DEFAULT (0),
    [MilestonesTotal] INT NOT NULL CONSTRAINT [DF_StarterProgress_MilestonesTotal] DEFAULT (0),
    [LastActivityDate] DATETIME2(7) NULL,
    [NextMilestoneDate] DATETIME2(7) NULL,
    [NextMilestoneName] NVARCHAR(200) NULL,
    [EngagementScore] DECIMAL(5,2) NULL, -- 0.00 to 100.00 based on activity
    [PerformanceScore] DECIMAL(5,2) NULL, -- 0.00 to 100.00 based on quality metrics
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_StarterProgress_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    CONSTRAINT [PK_StarterProgress] PRIMARY KEY CLUSTERED ([Id] ASC),
    -- Removed FK to AspNetUsers to maintain domain boundaries
    -- UserId is kept as column but without FK constraint
    CONSTRAINT [FK_StarterProgress_Projects] FOREIGN KEY ([ProjectId]) 
        REFERENCES [businessincubators].[Projects]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_StarterProgress_UserId_ProjectId] UNIQUE ([UserId], [ProjectId])
);
GO

-- Create indexes for progress queries
CREATE NONCLUSTERED INDEX [IX_StarterProgress_UserId] 
    ON [businessincubators].[StarterProgress] ([UserId]) 
    INCLUDE ([ProjectId], [CurrentPhase], [OverallProgress], [LastActivityDate]);
GO

CREATE NONCLUSTERED INDEX [IX_StarterProgress_ProjectId] 
    ON [businessincubators].[StarterProgress] ([ProjectId]) 
    INCLUDE ([UserId], [OverallProgress]);
GO

CREATE NONCLUSTERED INDEX [IX_StarterProgress_UpdatedAt] 
    ON [businessincubators].[StarterProgress] ([UpdatedAt] DESC) 
    WHERE [UpdatedAt] IS NOT NULL;
GO