CREATE TABLE [businessincubators].[Projects]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BusinessIncubatorId] BIGINT NOT NULL,
    [ExternalId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [Key] NVARCHAR(1000) NOT NULL,
    [SourceFormId] BIGINT NULL,
    
    [Status] INT NOT NULL CONSTRAINT [DF_Projects_Status] DEFAULT (1),
    
    -- Auditing
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(100) NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(100) NULL,

    -- Soft-delete
    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_Projects_IsDeleted] DEFAULT (0),
    [DeletedAt] DATETIME2 NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [RestoredAt] DATETIME2 NULL,
    [RestoredBy] NVARCHAR(100) NULL,

    -- Constraints
    CONSTRAINT [CHK_Projects_Status] CHECK ([Status] IN (1, 2)),

    FOREIGN KEY ([BusinessIncubatorId]) REFERENCES [businessincubators].[BusinessIncubators] ([Id])
    -- No FK for SourceFormId to allow orphaned references
)