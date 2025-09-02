-- Index for efficient sync queries on ProjectSubjects
CREATE NONCLUSTERED INDEX [IX_ProjectSubjects_SourceSubjectId]
ON [businessincubators].[ProjectSubjects] ([SourceSubjectId])
WHERE [SourceSubjectId] IS NOT NULL;