CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_ProjectStatus]
    ON [businessincubators].[ProjectFormSubmissions] ([ProjectId], [Status])
    INCLUDE ([ParticipantUserId], [FormId], [SubmittedAt]);