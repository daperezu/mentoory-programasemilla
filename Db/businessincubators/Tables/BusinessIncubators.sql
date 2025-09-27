CREATE TABLE [businessincubators].[BusinessIncubators]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ExternalId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [Key] NVARCHAR(1000) NOT NULL,
    [Status] INT NOT NULL CONSTRAINT [DF_BusinessIncubators_Status] DEFAULT (1),
    
    -- Auditing
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(100) NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(100) NULL,

    -- Soft-delete
    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_BusinessIncubators_IsDeleted] DEFAULT (0),
    [DeletedAt] DATETIME2 NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [RestoredAt] DATETIME2 NULL,
    [RestoredBy] NVARCHAR(100) NULL,

    -- Constraints
    CONSTRAINT [CHK_BusinessIncubators_Status] CHECK ([Status] IN (1, 2))
);