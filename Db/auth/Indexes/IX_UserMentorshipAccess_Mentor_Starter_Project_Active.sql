CREATE UNIQUE INDEX [IX_UserMentorshipAccess_Mentor_Starter_Project_Active]
ON [auth].[UserMentorshipAccess] ([MentorUserId], [StarterUserId], [ProjectId])
WHERE [IsActive] = 1;