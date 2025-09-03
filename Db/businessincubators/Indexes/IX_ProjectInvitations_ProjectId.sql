CREATE NONCLUSTERED INDEX [IX_ProjectInvitations_ProjectId]
    ON [businessincubators].[ProjectInvitations]([ProjectId])
    WHERE [IsDeleted] = 0;