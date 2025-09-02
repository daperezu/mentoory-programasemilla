-- Index for efficient sync queries on ProjectTopics
CREATE NONCLUSTERED INDEX [IX_ProjectTopics_SourceTopicId]
ON [businessincubators].[ProjectTopics] ([SourceTopicId])
WHERE [SourceTopicId] IS NOT NULL;