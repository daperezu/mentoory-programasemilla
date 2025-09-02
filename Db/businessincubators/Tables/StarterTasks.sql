CREATE TABLE [businessincubators].[StarterTasks] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Type] NVARCHAR(50) NOT NULL, -- 'form', 'document', 'meeting', 'training', 'milestone', 'custom'
    [Category] NVARCHAR(50) NULL, -- 'required', 'optional', 'recommended'
    [Priority] INT NOT NULL CONSTRAINT [DF_StarterTasks_Priority] DEFAULT (1), -- 0=low, 1=normal, 2=high, 3=urgent
    [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_StarterTasks_Status] DEFAULT ('pending'), -- 'pending', 'in_progress', 'completed', 'cancelled', 'overdue'
    [Phase] NVARCHAR(50) NULL, -- Related program phase
    [EstimatedDuration] INT NULL, -- Estimated duration in minutes
    [ActualDuration] INT NULL, -- Actual duration in minutes
    [DueDate] DATETIME2(7) NULL,
    [StartedAt] DATETIME2(7) NULL,
    [CompletedAt] DATETIME2(7) NULL,
    [CompletedBy] NVARCHAR(450) NULL,
    [CancelledAt] DATETIME2(7) NULL,
    [CancelledBy] NVARCHAR(450) NULL,
    [CancellationReason] NVARCHAR(500) NULL,
    [ActionUrl] NVARCHAR(500) NULL, -- URL to perform the task
    [ActionText] NVARCHAR(100) NULL, -- Button text for action
    [RelatedEntityType] NVARCHAR(100) NULL, -- 'Form', 'Document', etc.
    [RelatedEntityId] NVARCHAR(100) NULL,
    [Metadata] NVARCHAR(MAX) NULL, -- JSON additional data
    [Prerequisites] NVARCHAR(MAX) NULL, -- JSON array of prerequisite task IDs
    [DependentTasks] NVARCHAR(MAX) NULL, -- JSON array of dependent task IDs
    [RecurrenceRule] NVARCHAR(500) NULL, -- Recurrence pattern if applicable
    [ParentTaskId] BIGINT NULL, -- For subtasks
    [IsBlocking] BIT NOT NULL CONSTRAINT [DF_StarterTasks_IsBlocking] DEFAULT (0), -- Blocks other tasks
    [IsAutomated] BIT NOT NULL CONSTRAINT [DF_StarterTasks_IsAutomated] DEFAULT (0),
    [AutoCompleteCondition] NVARCHAR(500) NULL, -- Condition for auto-completion
    [ReminderSent] BIT NOT NULL CONSTRAINT [DF_StarterTasks_ReminderSent] DEFAULT (0),
    [ReminderSentAt] DATETIME2(7) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_StarterTasks_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(450) NOT NULL,
    [UpdatedAt] DATETIME2(7) NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_StarterTasks] PRIMARY KEY CLUSTERED ([Id] ASC),
    -- Removed FK to AspNetUsers to maintain domain boundaries
    -- UserId is kept as column but without FK constraint
    CONSTRAINT [FK_StarterTasks_Projects] FOREIGN KEY ([ProjectId]) 
        REFERENCES [businessincubators].[Projects]([Id]),
    CONSTRAINT [FK_StarterTasks_ParentTask] FOREIGN KEY ([ParentTaskId]) 
        REFERENCES [businessincubators].[StarterTasks]([Id])
);
GO

-- Create indexes for task queries
CREATE NONCLUSTERED INDEX [IX_StarterTasks_UserId_Status_DueDate] 
    ON [businessincubators].[StarterTasks] ([UserId], [Status], [DueDate]) 
    INCLUDE ([ProjectId], [Title], [Type], [Priority], [ActionUrl]);
GO

CREATE NONCLUSTERED INDEX [IX_StarterTasks_ProjectId_Status] 
    ON [businessincubators].[StarterTasks] ([ProjectId], [Status]) 
    INCLUDE ([UserId], [Title], [DueDate]);
GO

CREATE NONCLUSTERED INDEX [IX_StarterTasks_DueDate_Status] 
    ON [businessincubators].[StarterTasks] ([DueDate], [Status]) 
    WHERE [DueDate] IS NOT NULL AND [Status] IN ('pending', 'in_progress');
GO

CREATE NONCLUSTERED INDEX [IX_StarterTasks_ParentTaskId] 
    ON [businessincubators].[StarterTasks] ([ParentTaskId]) 
    WHERE [ParentTaskId] IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX [IX_StarterTasks_RelatedEntity] 
    ON [businessincubators].[StarterTasks] ([RelatedEntityType], [RelatedEntityId]) 
    WHERE [RelatedEntityType] IS NOT NULL AND [RelatedEntityId] IS NOT NULL;
GO