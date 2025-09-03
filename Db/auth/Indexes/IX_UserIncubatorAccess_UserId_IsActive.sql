CREATE NONCLUSTERED INDEX [IX_UserIncubatorAccess_UserId_IsActive]
ON [auth].[UserIncubatorAccess] ([UserId], [IsActive])
INCLUDE ([IncubatorId], [Role]);