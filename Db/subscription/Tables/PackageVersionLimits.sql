CREATE TABLE [subscription].[PackageVersionLimits]
(
    [PackageVersionId] BIGINT NOT NULL,
    [Type] INT NOT NULL,
    [Quantity] INT NOT NULL,
    CONSTRAINT [PK_PackageVersionLimits] PRIMARY KEY ([PackageVersionId], [Type]),
    CONSTRAINT [FK_PackageVersionLimits_PackageVersions] FOREIGN KEY ([PackageVersionId])
        REFERENCES [subscription].[PackageVersions]([Id]) ON DELETE CASCADE
);