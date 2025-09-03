CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_ProjectStageId]
ON [businessincubators].[ProjectFormSubmissions]([ProjectStageId])
WHERE [ProjectStageId] IS NOT NULL;