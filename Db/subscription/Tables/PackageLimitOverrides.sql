CREATE TABLE [subscription].[PackageLimitOverrides]
(
    [BusinessIncubatorPackageId] BIGINT NOT NULL,
    [Type] INT NOT NULL,
    [Quantity] INT NOT NULL,
    CONSTRAINT [PK_PackageLimitOverrides] PRIMARY KEY ([BusinessIncubatorPackageId], [Type], [Quantity]),
    CONSTRAINT [FK_PackageLimitOverrides_BusinessIncubatorPackages] FOREIGN KEY ([BusinessIncubatorPackageId])
        REFERENCES [subscription].[BusinessIncubatorPackages]([Id]) ON DELETE CASCADE
);
