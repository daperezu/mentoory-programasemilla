CREATE TABLE [core].[UserWidgetConfigurations] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserDashboardId] BIGINT NOT NULL,
    [WidgetId] BIGINT NOT NULL,
    [GridRow] INT NOT NULL CONSTRAINT [DF_UserWidgetConfigurations_GridRow] DEFAULT (0),
    [GridColumn] INT NOT NULL CONSTRAINT [DF_UserWidgetConfigurations_GridColumn] DEFAULT (0),
    [Width] INT NOT NULL CONSTRAINT [DF_UserWidgetConfigurations_Width] DEFAULT (1), -- Grid columns span
    [Height] INT NOT NULL CONSTRAINT [DF_UserWidgetConfigurations_Height] DEFAULT (1), -- Grid rows span
    [Size] NVARCHAR(20) NOT NULL CONSTRAINT [DF_UserWidgetConfigurations_Size] DEFAULT ('medium'), -- 'small', 'medium', 'large', 'full'
    [Configuration] NVARCHAR(MAX) NULL, -- JSON configuration overrides
    [IsVisible] BIT NOT NULL CONSTRAINT [DF_UserWidgetConfigurations_IsVisible] DEFAULT (1),
    [IsCollapsed] BIT NOT NULL CONSTRAINT [DF_UserWidgetConfigurations_IsCollapsed] DEFAULT (0),
    [RefreshInterval] INT NULL, -- Override widget default refresh interval (seconds)
    [LastRefreshedAt] DATETIME2(7) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_UserWidgetConfigurations_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    CONSTRAINT [PK_UserWidgetConfigurations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserWidgetConfigurations_UserDashboards] FOREIGN KEY ([UserDashboardId]) 
        REFERENCES [core].[UserDashboards]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserWidgetConfigurations_DashboardWidgets] FOREIGN KEY ([WidgetId]) 
        REFERENCES [core].[DashboardWidgets]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_UserWidgetConfigurations_Dashboard_Widget] UNIQUE ([UserDashboardId], [WidgetId])
);
GO

-- Create indexes for widget lookup
CREATE NONCLUSTERED INDEX [IX_UserWidgetConfigurations_UserDashboardId_IsVisible] 
    ON [core].[UserWidgetConfigurations] ([UserDashboardId], [IsVisible]) 
    INCLUDE ([WidgetId], [GridRow], [GridColumn], [Width], [Height], [Configuration]);
GO

CREATE NONCLUSTERED INDEX [IX_UserWidgetConfigurations_WidgetId] 
    ON [core].[UserWidgetConfigurations] ([WidgetId]);
GO