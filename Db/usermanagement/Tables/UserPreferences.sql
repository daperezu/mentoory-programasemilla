CREATE TABLE [usermanagement].[UserPreferences] (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserProfileId BIGINT NOT NULL,
    [Key] NVARCHAR(100) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL,
    
    CONSTRAINT FK_UserPreferences_UserProfiles FOREIGN KEY (UserProfileId) 
    REFERENCES [usermanagement].[UserProfiles](Id) ON DELETE CASCADE,
    
    CONSTRAINT UX_UserPreferences_UserProfileId_Key UNIQUE (UserProfileId, [Key])
);