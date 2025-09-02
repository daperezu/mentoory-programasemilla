-- Performance indexes for ProjectUsers table
CREATE NONCLUSTERED INDEX [IX_ProjectUsers_ProjectId_IsActive]
    ON [businessincubators].[ProjectUsers] ([ProjectId], [IsActive])
    INCLUDE ([UserId], [Role]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectUsers_UserId_IsActive]
    ON [businessincubators].[ProjectUsers] ([UserId], [IsActive])
    INCLUDE ([ProjectId], [Role]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectUsers_Role_IsActive]
    ON [businessincubators].[ProjectUsers] ([Role], [IsActive])
    INCLUDE ([ProjectId], [UserId]);