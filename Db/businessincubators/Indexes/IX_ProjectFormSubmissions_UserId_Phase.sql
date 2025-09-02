CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_UserId_Phase]
ON [businessincubators].[ProjectFormSubmissions]([ParticipantUserId], [Phase])
INCLUDE ([ProjectId], [Status], [CompletionPercentage]);