-- Index for efficient sync queries on ProjectQuestions
CREATE NONCLUSTERED INDEX [IX_ProjectQuestions_SourceQuestionId]
ON [businessincubators].[ProjectQuestions] ([SourceQuestionId])
WHERE [SourceQuestionId] IS NOT NULL;