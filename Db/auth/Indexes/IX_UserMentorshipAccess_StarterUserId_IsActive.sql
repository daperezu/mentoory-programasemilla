CREATE NONCLUSTERED INDEX [IX_UserMentorshipAccess_StarterUserId_IsActive]
ON [dbo].[UserMentorshipAccess] ([StarterUserId], [IsActive])
INCLUDE ([MentorUserId], [ProjectId], [IncubatorId]);