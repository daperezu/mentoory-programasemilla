CREATE TABLE [core].[UserDashboards] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [Role] NVARCHAR(450) NOT NULL,
    [Layout] NVARCHAR(MAX) NULL, -- JSON configuration for dashboard layout
    [Theme] NVARCHAR(50) NOT NULL CONSTRAINT [DF_UserDashboards_Theme] DEFAULT ('light'),
    [Language] NVARCHAR(10) NOT NULL CONSTRAINT [DF_UserDashboards_Language] DEFAULT ('es'),
    [RefreshInterval] INT NOT NULL CONSTRAINT [DF_UserDashboards_RefreshInterval] DEFAULT (300), -- seconds
    [IsActive] BIT NOT NULL CONSTRAINT [DF_UserDashboards_IsActive] DEFAULT (1),
    [CreatedDate] DATETIME2(7) NOT NULL CONSTRAINT [DF_UserDashboards_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [LastActivityDate] DATETIME2(7) NULL,
    -- DashboardPreferences owned type columns
    [PreferencesTheme] NVARCHAR(50) NULL,
    [PreferencesLanguage] NVARCHAR(10) NULL,
    [PreferencesRefreshInterval] INT NULL,
    [PreferencesShowNotifications] BIT NULL,
    [PreferencesPlayNotificationSound] BIT NULL,
    [PreferencesShowTaskReminders] BIT NULL,
    [PreferencesAutoRefreshEnabled] BIT NULL,
    [PreferencesCompactView] BIT NULL,
    [PreferencesShowWidgetHeaders] BIT NULL,
    [PreferencesEnableAnimations] BIT NULL,
    [PreferencesDateFormat] NVARCHAR(20) NULL,
    [PreferencesTimeFormat] NVARCHAR(20) NULL,
    [PreferencesTimezone] NVARCHAR(50) NULL,
    [PreferencesWidgetLayout] NVARCHAR(MAX) NULL,
    -- Audit columns
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_UserDashboards_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(450) NOT NULL,
    [UpdatedAt] DATETIME2(7) NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_UserDashboards] PRIMARY KEY CLUSTERED ([Id] ASC),
    -- Removed FK to AspNetUsers and AspNetRoles to maintain domain boundaries
    -- UserId and Role are kept as columns but without FK constraints
    CONSTRAINT [UQ_UserDashboards_UserId_Role] UNIQUE ([UserId], [Role])
);
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_UserDashboards_UserId] 
    ON [core].[UserDashboards] ([UserId]) 
    INCLUDE ([Role], [Layout], [Theme]);
GO

CREATE NONCLUSTERED INDEX [IX_UserDashboards_Role] 
    ON [core].[UserDashboards] ([Role]);
GO