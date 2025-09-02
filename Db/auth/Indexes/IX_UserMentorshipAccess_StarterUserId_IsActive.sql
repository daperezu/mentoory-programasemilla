CREATE NONCLUSTERED INDEX [IX_UserMentorshipAccess_StarterUserId_IsActive]
ON [auth].[UserMentorshipAccess] ([StarterUserId], [IsActive])
INCLUDE ([MentorUserId], [ProjectId], [IncubatorId]);