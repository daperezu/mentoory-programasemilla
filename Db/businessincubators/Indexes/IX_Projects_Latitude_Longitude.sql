CREATE NONCLUSTERED INDEX [IX_Projects_Latitude_Longitude]
ON [businessincubators].[Projects] ([Latitude], [Longitude])
WHERE [Latitude] IS NOT NULL AND [Longitude] IS NOT NULL AND [IsDeleted] = 0
INCLUDE ([Id], [ExternalId], [Name], [LocationName], [Geohash]);