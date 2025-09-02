CREATE UNIQUE INDEX [IX_BusinessIncubators_Key_Unique_Active]
ON [businessincubators].[BusinessIncubators] ([Key])
WHERE [IsDeleted] = 0;