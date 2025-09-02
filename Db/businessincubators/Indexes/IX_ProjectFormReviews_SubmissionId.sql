CREATE NONCLUSTERED INDEX [IX_ProjectFormReviews_SubmissionId]
    ON [businessincubators].[ProjectFormReviews]([SubmissionId] ASC)
    INCLUDE ([Status], [ReviewerId], [ReviewedAt]);