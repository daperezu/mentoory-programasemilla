CREATE TABLE [subscription].[PackageVersions]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [PackageId] BIGINT NOT NULL,
    [Label] NVARCHAR(50) NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [FK_PackageVersions_Packages] FOREIGN KEY ([PackageId])
        REFERENCES [subscription].[Packages]([Id]) ON DELETE CASCADE
);