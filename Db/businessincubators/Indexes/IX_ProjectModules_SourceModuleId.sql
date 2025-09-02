CREATE INDEX IX_ProjectModules_SourceModuleId 
ON businessincubators.ProjectModules(SourceModuleId) 
WHERE SourceModuleId IS NOT NULL;