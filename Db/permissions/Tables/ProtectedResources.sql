CREATE TABLE [permissions].ProtectedResources (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    ExternalId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),  -- Used for linking different entity types
    ResourceType INT NOT NULL,  -- '1:Project', '2:WebFeature'
    Name NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255) NOT NULL DEFAULT '',
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(255) NULL,
    UNIQUE (ExternalId)  -- Ensures each ExternalId is unique across entities likes projects, webfeatures, and so on.
);
