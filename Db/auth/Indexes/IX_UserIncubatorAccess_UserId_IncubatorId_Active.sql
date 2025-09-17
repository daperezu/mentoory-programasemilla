CREATE UNIQUE INDEX [IX_UserIncubatorAccess_UserId_IncubatorId_Active]
ON [dbo].[UserIncubatorAccess] ([UserId], [IncubatorId])
WHERE [IsActive] = 1;