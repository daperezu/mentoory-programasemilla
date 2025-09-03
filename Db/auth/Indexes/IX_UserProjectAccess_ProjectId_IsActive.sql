CREATE NONCLUSTERED INDEX [IX_UserProjectAccess_ProjectId_IsActive]
ON [auth].[UserProjectAccess] ([ProjectId], [IsActive])
INCLUDE ([UserId], [Role]);