CREATE NONCLUSTERED INDEX [IX_UserProjectAccess_UserId_IsActive]
ON [dbo].[UserProjectAccess] ([UserId], [IsActive])
INCLUDE ([ProjectId], [IncubatorId], [Role]);