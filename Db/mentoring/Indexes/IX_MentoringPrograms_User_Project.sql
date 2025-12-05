CREATE NONCLUSTERED INDEX IX_Programs_User_Project
    ON [mentoring].[Programs] ([UserId], [ProjectId]);
