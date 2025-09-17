CREATE NONCLUSTERED INDEX [IX_UserIncubatorAccess_UserId_IsActive]
ON [dbo].[UserIncubatorAccess] ([UserId], [IsActive])
INCLUDE ([IncubatorId], [Role]);