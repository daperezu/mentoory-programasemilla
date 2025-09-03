CREATE NONCLUSTERED INDEX [IX_UserMentorshipAccess_ProjectId_IsActive]
ON [auth].[UserMentorshipAccess] ([ProjectId], [IsActive])
INCLUDE ([MentorUserId], [StarterUserId]);