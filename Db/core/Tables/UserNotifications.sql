CREATE TABLE [core].[UserNotifications] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL, -- 'system', 'task', 'form', 'message', 'achievement', 'reminder'
    [Category] NVARCHAR(50) NOT NULL, -- 'info', 'success', 'warning', 'error'
    [Priority] INT NOT NULL CONSTRAINT [DF_UserNotifications_Priority] DEFAULT (0), -- 0=low, 1=normal, 2=high, 3=urgent
    [Title] NVARCHAR(200) NOT NULL,
    [Message] NVARCHAR(MAX) NOT NULL,
    [Data] NVARCHAR(MAX) NULL, -- JSON additional data
    [ActionUrl] NVARCHAR(500) NULL, -- URL to navigate when clicked
    [ActionText] NVARCHAR(100) NULL, -- Text for action button
    [IconClass] NVARCHAR(100) NULL, -- Bootstrap icon class
    [IsRead] BIT NOT NULL CONSTRAINT [DF_UserNotifications_IsRead] DEFAULT (0),
    [IsDismissed] BIT NOT NULL CONSTRAINT [DF_UserNotifications_IsDismissed] DEFAULT (0),
    [IsActionTaken] BIT NOT NULL CONSTRAINT [DF_UserNotifications_IsActionTaken] DEFAULT (0),
    [ExpiresAt] DATETIME2(7) NULL,
    [ReadAt] DATETIME2(7) NULL,
    [DismissedAt] DATETIME2(7) NULL,
    [ActionTakenAt] DATETIME2(7) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_UserNotifications_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(450) NULL, -- System or user who created the notification
    CONSTRAINT [PK_UserNotifications] PRIMARY KEY CLUSTERED ([Id] ASC)
    -- Removed FK to AspNetUsers to maintain domain boundaries
    -- UserId is kept as column but without FK constraint
);
GO

-- Create indexes for notification queries
CREATE NONCLUSTERED INDEX [IX_UserNotifications_UserId_IsRead_IsDismissed_CreatedAt] 
    ON [core].[UserNotifications] ([UserId], [IsRead], [IsDismissed], [CreatedAt] DESC) 
    INCLUDE ([Type], [Category], [Priority], [Title], [Message]);
GO

CREATE NONCLUSTERED INDEX [IX_UserNotifications_UserId_Priority_CreatedAt] 
    ON [core].[UserNotifications] ([UserId], [Priority] DESC, [CreatedAt] DESC) 
    WHERE [IsRead] = 0 AND [IsDismissed] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_UserNotifications_ExpiresAt] 
    ON [core].[UserNotifications] ([ExpiresAt]) 
    WHERE [ExpiresAt] IS NOT NULL AND [IsDismissed] = 0;
GO