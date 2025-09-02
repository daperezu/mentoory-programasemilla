CREATE NONCLUSTERED INDEX [IX_ProjectInvitations_ExpiresAt]
    ON [businessincubators].[ProjectInvitations]([ExpiresAt])
    WHERE [IsDeleted] = 0 AND [Status] = 0; -- Only for pending invitations