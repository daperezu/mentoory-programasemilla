CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_ParticipantUser]
    ON [businessincubators].[ProjectFormSubmissions] ([ParticipantUserId])
    INCLUDE ([ProjectId], [FormId], [Status]);