-- Unique per active record
CREATE UNIQUE INDEX [IX_BusinessIncubators_Name_Unique_Active]
ON [businessincubators].[BusinessIncubators] ([Name])
WHERE [IsDeleted] = 0;