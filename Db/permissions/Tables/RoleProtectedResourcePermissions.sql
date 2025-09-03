CREATE TABLE [permissions].RoleProtectedResourcePermissions (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Role] NVARCHAR(256) NOT NULL,  
    ProtectedResourceId BIGINT NOT NULL,
    FOREIGN KEY (ProtectedResourceId) REFERENCES [permissions].ProtectedResources(Id) ON DELETE CASCADE
);
