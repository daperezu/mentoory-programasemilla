-- Performance indexes for ProjectUsers table
CREATE NONCLUSTERED INDEX [IX_ProjectUsers_ProjectId_IsActive]
    ON [businessincubators].[ProjectUsers] ([ProjectId], [IsActive])
    INCLUDE ([UserId], [Role], [JoinedAt]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectUsers_UserId_IsActive]
    ON [businessincubators].[ProjectUsers] ([UserId], [IsActive])
    INCLUDE ([ProjectId], [Role]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectUsers_Role_IsActive]
    ON [businessincubators].[ProjectUsers] ([Role], [IsActive])
    INCLUDE ([ProjectId], [UserId]);
GO

-- Dashboard specific performance index
CREATE NONCLUSTERED INDEX [IX_ProjectUsers_Dashboard]
    ON [businessincubators].[ProjectUsers] ([ProjectId], [IsActive], [JoinedAt])
    INCLUDE ([UserId], [Role]);
GO