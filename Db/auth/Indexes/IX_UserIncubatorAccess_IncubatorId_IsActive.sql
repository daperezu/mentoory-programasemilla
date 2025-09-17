CREATE NONCLUSTERED INDEX [IX_UserIncubatorAccess_IncubatorId_IsActive]
ON [dbo].[UserIncubatorAccess] ([IncubatorId], [IsActive])
INCLUDE ([UserId], [Role]);