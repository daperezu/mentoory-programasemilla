CREATE TABLE [core].[UserActivities] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL, -- 'login', 'logout', 'form_submit', 'task_complete', 'document_access', etc.
    [Category] NVARCHAR(50) NOT NULL, -- 'authentication', 'form', 'task', 'document', 'communication', etc.
    [Action] NVARCHAR(100) NOT NULL, -- Specific action performed
    [Description] NVARCHAR(500) NOT NULL,
    [EntityType] NVARCHAR(100) NULL, -- Related entity type (e.g., 'Project', 'Form', 'Task')
    [EntityId] NVARCHAR(100) NULL, -- Related entity ID
    [Metadata] NVARCHAR(MAX) NULL, -- JSON additional metadata
    [IpAddress] NVARCHAR(45) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [SessionId] NVARCHAR(100) NULL,
    [Duration] INT NULL, -- Duration in seconds for timed activities
    [IsSuccessful] BIT NOT NULL CONSTRAINT [DF_UserActivities_IsSuccessful] DEFAULT (1),
    [ErrorMessage] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_UserActivities_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_UserActivities] PRIMARY KEY CLUSTERED ([Id] ASC)
    -- Removed FK to AspNetUsers to maintain domain boundaries
    -- UserId is kept as column but without FK constraint
);
GO

-- Create indexes for activity queries
CREATE NONCLUSTERED INDEX [IX_UserActivities_UserId_CreatedAt] 
    ON [core].[UserActivities] ([UserId], [CreatedAt] DESC) 
    INCLUDE ([Type], [Category], [Action], [Description]);
GO

CREATE NONCLUSTERED INDEX [IX_UserActivities_UserId_Type_CreatedAt] 
    ON [core].[UserActivities] ([UserId], [Type], [CreatedAt] DESC);
GO

CREATE NONCLUSTERED INDEX [IX_UserActivities_EntityType_EntityId] 
    ON [core].[UserActivities] ([EntityType], [EntityId]) 
    WHERE [EntityType] IS NOT NULL AND [EntityId] IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX [IX_UserActivities_CreatedAt] 
    ON [core].[UserActivities] ([CreatedAt] DESC) 
    INCLUDE ([UserId], [Type], [Category]);
GO