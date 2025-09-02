CREATE NONCLUSTERED INDEX [IX_BatchUserRegistrations_CreatedAt]
    ON [businessincubators].[BatchUserRegistrations]([CreatedAt] DESC)
    WHERE [IsDeleted] = 0;