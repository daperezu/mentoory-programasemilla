CREATE TABLE [dbo].[UserContextPreferences] (
    [UserId] NVARCHAR(450) NOT NULL,
    [LastRole] NVARCHAR(256) NULL,
    [LastIncubatorId] BIGINT NULL,
    [LastProjectId] BIGINT NULL,
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [PK_UserContextPreferences] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_UserContextPreferences_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    -- Removed FK to businessincubators schema to maintain domain boundaries
    -- LastIncubatorId and LastProjectId are kept as columns but without FK constraints
);