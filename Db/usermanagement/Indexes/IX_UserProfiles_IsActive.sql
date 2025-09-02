CREATE INDEX IX_UserProfiles_IsActive 
ON [usermanagement].[UserProfiles] (IsActive) 
WHERE IsActive = 1;