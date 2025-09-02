CREATE INDEX [IX_ReportSchedules_NextRunAt_Active]
ON [businessincubators].[ReportSchedules] ([NextRunAt], [IsActive])
WHERE [IsDeleted] = 0;