CREATE NONCLUSTERED INDEX IX_Sessions_Program
    ON [mentoring].[Sessions] ([ProgramId], [ScheduledAt]);
