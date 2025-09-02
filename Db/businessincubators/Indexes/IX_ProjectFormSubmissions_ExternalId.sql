CREATE UNIQUE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_ExternalId]
    ON [businessincubators].[ProjectFormSubmissions]([ExternalId])
    INCLUDE ([ProjectId], [ParticipantUserId], [FormId], [Status], [Phase]);