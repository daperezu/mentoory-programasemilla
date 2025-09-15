CREATE NONCLUSTERED INDEX [IX_UserMentorshipAccess_ProjectId_IsActive]
ON [dbo].[UserMentorshipAccess] ([ProjectId], [IsActive])
INCLUDE ([MentorUserId], [StarterUserId]);