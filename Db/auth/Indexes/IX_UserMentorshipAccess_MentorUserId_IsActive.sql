CREATE NONCLUSTERED INDEX [IX_UserMentorshipAccess_MentorUserId_IsActive]
ON [auth].[UserMentorshipAccess] ([MentorUserId], [IsActive])
INCLUDE ([StarterUserId], [ProjectId], [IncubatorId]);