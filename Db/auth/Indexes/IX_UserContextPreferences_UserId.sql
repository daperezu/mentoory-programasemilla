CREATE NONCLUSTERED INDEX [IX_UserContextPreferences_UserId]
    ON [dbo].[UserContextPreferences]([UserId])
    INCLUDE ([LastRole], [LastIncubatorId], [LastProjectId]);