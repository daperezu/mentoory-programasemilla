CREATE NONCLUSTERED INDEX [IX_ProjectInvitations_Email_Status]
    ON [businessincubators].[ProjectInvitations]([Email], [Status])
    WHERE [IsDeleted] = 0;