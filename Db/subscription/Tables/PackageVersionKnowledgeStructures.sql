CREATE TABLE [subscription].[PackageVersionKnowledgeStructures]
(
    [PackageVersionId] BIGINT NOT NULL,
    [KnowledgeStructureId] BIGINT NOT NULL,
    
    CONSTRAINT [PK_PackageVersionKnowledgeStructures] PRIMARY KEY CLUSTERED ([KnowledgeStructureId], [PackageVersionId]),

    CONSTRAINT [FK_PackageVersionKnowledgeStructures_PackageVersions] FOREIGN KEY ([PackageVersionId])
        REFERENCES [subscription].[PackageVersions]([Id]) ON DELETE CASCADE
);
