CREATE NONCLUSTERED INDEX [IX_UserProjectAccess_ProjectId_IsActive]
ON [dbo].[UserProjectAccess] ([ProjectId], [IsActive])
INCLUDE ([UserId], [Role]);