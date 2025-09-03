CREATE TABLE [businessincubators].[ProjectUsers]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL, -- 'Starter', 'Mentor', 'Coordinator', etc.
    [IsActive] BIT NOT NULL DEFAULT 1,
    [JoinedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LeftAt] DATETIME2 NULL,
    [InvitedBy] NVARCHAR(450) NULL,
    [Metadata] NVARCHAR(MAX) NULL, -- JSON for additional role-specific data
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_ProjectUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProjectUsers_Projects] FOREIGN KEY ([ProjectId]) 
        REFERENCES [businessincubators].[Projects]([Id]),
    -- Removed FK to AspNetUsers to maintain domain boundaries
    -- UserId and InvitedBy are kept as columns but without FK constraints
    CONSTRAINT [UQ_ProjectUsers_Project_User_Role] UNIQUE ([ProjectId], [UserId], [Role])
);