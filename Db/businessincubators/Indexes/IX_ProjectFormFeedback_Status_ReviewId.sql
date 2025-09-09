CREATE NONCLUSTERED INDEX [IX_ProjectFormFeedback_Status_ReviewId]
ON [businessincubators].[ProjectFormFeedback]([Status], [ReviewId])
INCLUDE ([BlockId], [QuestionId], [IsFromParticipant]);