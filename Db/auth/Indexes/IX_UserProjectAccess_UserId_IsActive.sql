CREATE NONCLUSTERED INDEX [IX_UserProjectAccess_UserId_IsActive]
ON [auth].[UserProjectAccess] ([UserId], [IsActive])
INCLUDE ([ProjectId], [IncubatorId], [Role]);