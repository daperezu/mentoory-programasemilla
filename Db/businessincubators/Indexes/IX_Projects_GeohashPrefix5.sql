CREATE NONCLUSTERED INDEX [IX_Projects_GeohashPrefix5]
ON [businessincubators].[Projects] ([GeohashPrefix5])
WHERE [GeohashPrefix5] IS NOT NULL AND [IsDeleted] = 0
INCLUDE ([Id], [ExternalId], [Name], [Latitude], [Longitude], [LocationName]);