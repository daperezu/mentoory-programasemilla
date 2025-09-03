CREATE TABLE [notification].[EmailTemplates]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_notification_EmailTemplates] PRIMARY KEY,
    [Key] NVARCHAR(100) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Subject] NVARCHAR(500) NOT NULL,
    [BodyHtml] NVARCHAR(MAX) NOT NULL,
    [BodyText] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(500) NULL,
    [Category] NVARCHAR(100) NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_notification_EmailTemplates_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL,
    [UpdatedAt] DATETIME2(7) NOT NULL,
    [Language] NVARCHAR(10) NULL
);
GO

-- Create unique index on Key
CREATE UNIQUE INDEX [IX_notification_EmailTemplates_Key] ON [notification].[EmailTemplates] ([Key]);
GO