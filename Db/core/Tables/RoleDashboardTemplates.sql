CREATE TABLE [core].[RoleDashboardTemplates] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Role] NVARCHAR(256) NOT NULL,
    [RoleName] NVARCHAR(100) NOT NULL CONSTRAINT [DF_RoleDashboardTemplates_RoleName] DEFAULT (''),
    [DefaultLayout] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RoleDashboardTemplates_DefaultLayout] DEFAULT ('[]'),
    [DefaultTheme] NVARCHAR(50) NOT NULL CONSTRAINT [DF_RoleDashboardTemplates_DefaultTheme] DEFAULT ('light'),
    [DefaultLanguage] NVARCHAR(10) NOT NULL CONSTRAINT [DF_RoleDashboardTemplates_DefaultLanguage] DEFAULT ('es'),
    [DefaultRefreshInterval] INT NOT NULL CONSTRAINT [DF_RoleDashboardTemplates_DefaultRefreshInterval] DEFAULT (300), -- seconds
    [WidgetCodes] NVARCHAR(MAX) NULL, -- Comma-separated widget codes
    [IsActive] BIT NOT NULL CONSTRAINT [DF_RoleDashboardTemplates_IsActive] DEFAULT (1),
    [CreatedDate] DATETIME2(7) NOT NULL CONSTRAINT [DF_RoleDashboardTemplates_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [ModifiedDate] DATETIME2(7) NULL,
    -- Legacy columns for backward compatibility (if needed)
    [Name] NVARCHAR(100) NULL,
    [Description] NVARCHAR(500) NULL,
    [WidgetLayout] NVARCHAR(MAX) NULL,
    [GridColumns] INT NULL,
    [GridRows] INT NULL,
    [RefreshInterval] INT NULL,
    [IsDefault] BIT NULL,
    [Version] INT NULL,
    [CreatedAt] DATETIME2(7) NULL,
    [CreatedBy] NVARCHAR(450) NULL,
    [UpdatedAt] DATETIME2(7) NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_RoleDashboardTemplates] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Create indexes for template lookup
CREATE NONCLUSTERED INDEX [IX_RoleDashboardTemplates_Role_IsActive] 
    ON [core].[RoleDashboardTemplates] ([Role], [IsActive]) 
    INCLUDE ([RoleName], [DefaultLayout], [DefaultTheme]);
GO