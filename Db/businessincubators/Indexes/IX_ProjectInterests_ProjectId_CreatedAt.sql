CREATE NONCLUSTERED INDEX [IX_ProjectInterests_ProjectId_CreatedAt]
ON [businessincubators].[ProjectInterests] ([ProjectId], [CreatedAt] DESC)
INCLUDE ([ObserverUserId], [InterestType]);