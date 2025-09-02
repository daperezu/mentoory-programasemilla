CREATE NONCLUSTERED INDEX [IX_BatchUserRegistrations_ProjectId_Status]
    ON [businessincubators].[BatchUserRegistrations]([ProjectId], [Status])
    WHERE [IsDeleted] = 0;