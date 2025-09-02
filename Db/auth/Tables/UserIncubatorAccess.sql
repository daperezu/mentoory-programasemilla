CREATE TABLE [dbo].[UserIncubatorAccess]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [IncubatorId] BIGINT NOT NULL,
    [Role] NVARCHAR(256) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [LastSyncedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_UserIncubatorAccess] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserIncubatorAccess_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);