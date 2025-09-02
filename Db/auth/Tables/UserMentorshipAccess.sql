CREATE TABLE [dbo].[UserMentorshipAccess]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [MentorUserId] NVARCHAR(450) NOT NULL,
    [StarterUserId] NVARCHAR(450) NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [IncubatorId] BIGINT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [AssignedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [EndedAt] DATETIME2 NULL,
    [LastSyncedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_UserMentorshipAccess] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserMentorshipAccess_MentorUser] FOREIGN KEY ([MentorUserId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_UserMentorshipAccess_StarterUser] FOREIGN KEY ([StarterUserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);