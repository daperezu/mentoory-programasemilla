CREATE UNIQUE INDEX [IX_UserProjectAccess_UserId_ProjectId_Active]
ON [auth].[UserProjectAccess] ([UserId], [ProjectId])
WHERE [IsActive] = 1;