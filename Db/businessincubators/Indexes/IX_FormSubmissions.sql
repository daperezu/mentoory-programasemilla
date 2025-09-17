-- Performance indexes for ProjectFormSubmissions table (Dashboard optimization)
CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_ProjectId_Status]
    ON [businessincubators].[ProjectFormSubmissions] ([ProjectId], [Status])
    INCLUDE ([ParticipantUserId], [StartedAt], [SubmittedAt]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_ProjectId_SubmittedAt]
    ON [businessincubators].[ProjectFormSubmissions] ([ProjectId], [SubmittedAt])
    WHERE [SubmittedAt] IS NOT NULL
    INCLUDE ([ParticipantUserId], [Status], [StartedAt]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_Dashboard]
    ON [businessincubators].[ProjectFormSubmissions] ([ProjectId])
    INCLUDE ([Status], [ParticipantUserId], [StartedAt], [SubmittedAt]);
GO