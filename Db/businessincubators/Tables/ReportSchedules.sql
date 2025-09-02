CREATE TABLE [businessincubators].[ReportSchedules]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [ExternalId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [TemplateId] BIGINT NOT NULL,
    [CronExpression] NVARCHAR(100) NOT NULL,
    [Recipients] NVARCHAR(MAX) NOT NULL, -- JSON array of email addresses
    [IsActive] BIT NOT NULL DEFAULT 1,
    [LastRunAt] DATETIME2 NULL,
    [NextRunAt] DATETIME2 NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(450) NOT NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2 NULL,
    [DeletedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_ReportSchedules] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ReportSchedules_ReportTemplates] FOREIGN KEY ([TemplateId]) 
        REFERENCES [businessincubators].[ReportTemplates]([Id])
);