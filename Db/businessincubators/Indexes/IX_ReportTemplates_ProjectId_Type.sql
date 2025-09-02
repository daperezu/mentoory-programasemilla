CREATE INDEX [IX_ReportTemplates_ProjectId_Type]
ON [businessincubators].[ReportTemplates] ([ProjectId], [Type])
INCLUDE ([Name], [IsGlobal], [CreatedAt])
WHERE [IsDeleted] = 0;