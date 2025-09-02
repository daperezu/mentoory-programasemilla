CREATE TABLE [core].[DashboardWidgets] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [DisplayName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [Type] NVARCHAR(50) NOT NULL, -- 'kpi', 'chart', 'list', 'calendar', 'feed', etc.
    [Component] NVARCHAR(200) NOT NULL, -- Component path or identifier
    [IconClass] NVARCHAR(100) NULL, -- Bootstrap icon class
    [DefaultConfig] NVARCHAR(MAX) NULL, -- JSON default configuration
    [Roles] NVARCHAR(MAX) NOT NULL, -- JSON array of role names that can use this widget
    [MinSize] NVARCHAR(20) NOT NULL CONSTRAINT [DF_DashboardWidgets_MinSize] DEFAULT ('small'), -- 'small', 'medium', 'large', 'full'
    [MaxSize] NVARCHAR(20) NOT NULL CONSTRAINT [DF_DashboardWidgets_MaxSize] DEFAULT ('full'),
    [IsResizable] BIT NOT NULL CONSTRAINT [DF_DashboardWidgets_IsResizable] DEFAULT (1),
    [IsDraggable] BIT NOT NULL CONSTRAINT [DF_DashboardWidgets_IsDraggable] DEFAULT (1),
    [Refreshable] BIT NOT NULL CONSTRAINT [DF_DashboardWidgets_Refreshable] DEFAULT (1),
    [RefreshIntervalSeconds] INT NULL,
    [SortOrder] INT NOT NULL CONSTRAINT [DF_DashboardWidgets_SortOrder] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_DashboardWidgets_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_DashboardWidgets_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    CONSTRAINT [PK_DashboardWidgets] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_DashboardWidgets_Name] UNIQUE ([Name])
);
GO

-- Create index for active widgets lookup
CREATE NONCLUSTERED INDEX [IX_DashboardWidgets_IsActive_SortOrder] 
    ON [core].[DashboardWidgets] ([IsActive], [SortOrder]) 
    INCLUDE ([Name], [Type], [Component], [Roles]);
GO