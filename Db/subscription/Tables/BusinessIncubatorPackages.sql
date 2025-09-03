CREATE TABLE [subscription].[BusinessIncubatorPackages]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [BusinessIncubatorId] BIGINT NOT NULL, -- FK to BusinessIncubator.Id
    [PackageVersionId] BIGINT NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [FK_BusinessIncubatorPackages_PackageVersions] FOREIGN KEY ([PackageVersionId])
        REFERENCES [subscription].[PackageVersions]([Id]) ON DELETE NO ACTION
    -- FK to [BusinessIncubator].[BusinessIncubators] can be added here if the external domain is in the same project
);