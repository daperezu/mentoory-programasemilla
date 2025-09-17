CREATE NONCLUSTERED INDEX [IX_Projects_GeohashPrefix6]
ON [businessincubators].[Projects] ([GeohashPrefix6])
WHERE [GeohashPrefix6] IS NOT NULL AND [IsDeleted] = 0
INCLUDE ([Id], [ExternalId], [Name], [Latitude], [Longitude], [LocationName]);