CREATE NONCLUSTERED INDEX [IX_UserMentorshipAccess_MentorUserId_IsActive]
ON [dbo].[UserMentorshipAccess] ([MentorUserId], [IsActive])
INCLUDE ([StarterUserId], [ProjectId], [IncubatorId]);