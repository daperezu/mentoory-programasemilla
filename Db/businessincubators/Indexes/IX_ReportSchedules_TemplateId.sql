CREATE INDEX [IX_ReportSchedules_TemplateId]
ON [businessincubators].[ReportSchedules] ([TemplateId])
WHERE [IsDeleted] = 0;