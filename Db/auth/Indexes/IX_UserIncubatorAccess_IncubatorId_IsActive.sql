CREATE NONCLUSTERED INDEX [IX_UserIncubatorAccess_IncubatorId_IsActive]
ON [auth].[UserIncubatorAccess] ([IncubatorId], [IsActive])
INCLUDE ([UserId], [Role]);