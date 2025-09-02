CREATE TABLE [permissions].UserProtectedResourcePermissions (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,  
    ProtectedResourceId BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255) NOT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(255) NULL,
    FOREIGN KEY (ProtectedResourceId) REFERENCES [permissions].ProtectedResources(Id) ON DELETE CASCADE
);
