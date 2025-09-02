CREATE TABLE [businessincubators].[StarterResources]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [Category] NVARCHAR(50) NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NOT NULL,
    [ResourceType] NVARCHAR(50) NOT NULL,
    [Url] NVARCHAR(500) NULL,
    [FilePath] NVARCHAR(500) NULL,
    [ThumbnailUrl] NVARCHAR(500) NULL,
    [Phase] NVARCHAR(50) NOT NULL,
    [Order] INT NOT NULL DEFAULT 0,
    [IsRequired] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [ViewCount] INT NOT NULL DEFAULT 0,
    [LastViewedDate] DATETIME2(7) NULL,
    [LastViewedBy] NVARCHAR(450) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(450) NOT NULL,
    [LastModifiedAt] DATETIME2(7) NULL,
    [LastModifiedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_StarterResources] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StarterResources_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [businessincubators].[Projects] ([Id])
);
GO

-- Create indexes for better query performance
CREATE NONCLUSTERED INDEX [IX_StarterResources_ProjectId_Phase]
ON [businessincubators].[StarterResources] ([ProjectId], [Phase])
WHERE [IsActive] = 1;
GO

CREATE NONCLUSTERED INDEX [IX_StarterResources_Category]
ON [businessincubators].[StarterResources] ([Category])
WHERE [IsActive] = 1;
GO

CREATE NONCLUSTERED INDEX [IX_StarterResources_ResourceType]
ON [businessincubators].[StarterResources] ([ResourceType])
WHERE [IsActive] = 1;