CREATE TABLE [subscription].[PackageKnowledgeStructureOverride]
(
    [BusinessIncubatorPackageId] BIGINT NOT NULL,
    [KnowledgeStructureId] BIGINT NOT NULL,

    FOREIGN KEY ([BusinessIncubatorPackageId]) REFERENCES [subscription].[BusinessIncubatorPackages]([Id]),
);
