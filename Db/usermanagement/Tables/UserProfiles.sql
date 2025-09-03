CREATE TABLE [usermanagement].[UserProfiles] (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Identification NVARCHAR(50) NOT NULL,
    Country NVARCHAR(100) NULL,
    Province NVARCHAR(100) NULL,
    Canton NVARCHAR(100) NULL,
    District NVARCHAR(100) NULL,
    FullAddress NVARCHAR(500) NULL,
    AvatarUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    
    CONSTRAINT FK_UserProfiles_AspNetUsers FOREIGN KEY (UserId) 
    REFERENCES [dbo].[AspNetUsers](Id) ON DELETE CASCADE,
    
    CONSTRAINT UX_UserProfiles_UserId UNIQUE (UserId),
    CONSTRAINT UX_UserProfiles_Identification UNIQUE (Identification)
);