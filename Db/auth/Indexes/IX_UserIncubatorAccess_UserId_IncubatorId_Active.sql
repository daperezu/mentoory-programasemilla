CREATE UNIQUE INDEX [IX_UserIncubatorAccess_UserId_IncubatorId_Active]
ON [auth].[UserIncubatorAccess] ([UserId], [IncubatorId])
WHERE [IsActive] = 1;