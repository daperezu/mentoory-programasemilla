-- Index for efficient sync queries on ProjectAnswerOptions
CREATE NONCLUSTERED INDEX [IX_ProjectAnswerOptions_SourceAnswerOptionId]
ON [businessincubators].[ProjectAnswerOptions] ([SourceAnswerOptionId])
WHERE [SourceAnswerOptionId] IS NOT NULL;