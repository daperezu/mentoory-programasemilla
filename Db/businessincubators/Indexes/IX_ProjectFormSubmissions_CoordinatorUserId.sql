-- Index for coordinator queries on ProjectFormSubmissions
CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_CoordinatorUserId]
ON [businessincubators].[ProjectFormSubmissions]([CoordinatorUserId])
WHERE [CoordinatorUserId] IS NOT NULL;