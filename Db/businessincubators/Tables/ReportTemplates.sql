CREATE TABLE [businessincubators].[ReportTemplates]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [ExternalId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [Type] INT NOT NULL, -- 0=Progress, 1=Completion, 2=Participation, 3=Custom
    [IsGlobal] BIT NOT NULL DEFAULT 0,
    [ProjectId] BIGINT NULL,
    [ConfigurationJson] NVARCHAR(MAX) NOT NULL, -- Report settings and layout
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(450) NOT NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2 NULL,
    [DeletedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_ReportTemplates] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ReportTemplates_Projects] FOREIGN KEY ([ProjectId]) 
        REFERENCES [businessincubators].[Projects]([Id])
);