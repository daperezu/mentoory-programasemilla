CREATE NONCLUSTERED INDEX [IX_ProjectFormFeedback_ReviewId]
    ON [businessincubators].[ProjectFormFeedback]([ReviewId] ASC)
    INCLUDE ([BlockId], [QuestionId], [FeedbackType]);