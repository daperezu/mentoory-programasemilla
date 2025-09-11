-- Index for source-based queries on DiagnosisAnswers
CREATE NONCLUSTERED INDEX [IX_DiagnosisAnswers_AnswerSource]
ON [diagnostics].[DiagnosisAnswers]([AnswerSource])
INCLUDE ([ProjectId], [UserId], [Phase]);