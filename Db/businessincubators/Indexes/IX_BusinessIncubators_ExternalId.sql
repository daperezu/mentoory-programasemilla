-- Public ID safety
CREATE UNIQUE INDEX [IX_BusinessIncubators_ExternalId]
ON [businessincubators].[BusinessIncubators] ([ExternalId]);