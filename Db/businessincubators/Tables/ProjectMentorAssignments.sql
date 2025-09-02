CREATE TABLE [businessincubators].[ProjectMentorAssignments] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [MentorUserId] NVARCHAR(450) NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [StarterUserId] NVARCHAR(450) NOT NULL,
    [AssignedDate] DATETIME2(7) NOT NULL CONSTRAINT [DF_ProjectMentorAssignments_AssignedDate] DEFAULT (SYSUTCDATETIME()),
    [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ProjectMentorAssignments_Status] DEFAULT ('active'),
    [MeetingSchedule] NVARCHAR(MAX) NULL, -- JSON schedule data
    [TotalSessions] INT NOT NULL CONSTRAINT [DF_ProjectMentorAssignments_TotalSessions] DEFAULT (0),
    [CompletedSessions] INT NOT NULL CONSTRAINT [DF_ProjectMentorAssignments_CompletedSessions] DEFAULT (0),
    [NextSessionDate] DATETIME2(7) NULL,
    [LastSessionDate] DATETIME2(7) NULL,
    [SessionNotes] NVARCHAR(MAX) NULL,
    [MentorSpecialties] NVARCHAR(500) NULL,
    [PreferredMeetingType] NVARCHAR(50) NULL, -- 'virtual', 'in-person', 'hybrid'
    [Rating] DECIMAL(3,2) NULL, -- 0.00 to 5.00
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_ProjectMentorAssignments_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(450) NOT NULL,
    [UpdatedAt] DATETIME2(7) NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_ProjectMentorAssignments] PRIMARY KEY CLUSTERED ([Id] ASC),
    -- Removed FK to AspNetUsers to maintain domain boundaries
    -- MentorUserId and StarterUserId are kept as columns but without FK constraints
    CONSTRAINT [FK_ProjectMentorAssignments_Project] FOREIGN KEY ([ProjectId]) 
        REFERENCES [businessincubators].[Projects]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ProjectMentorAssignment] UNIQUE ([ProjectId], [StarterUserId]),
    CONSTRAINT [CHK_ProjectMentorAssignments_Rating] CHECK ([Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 5)),
    CONSTRAINT [CHK_ProjectMentorAssignments_Sessions] CHECK ([CompletedSessions] <= [TotalSessions])
);
GO

-- Create indexes for mentor assignment queries
CREATE NONCLUSTERED INDEX [IX_ProjectMentorAssignments_MentorUserId] 
    ON [businessincubators].[ProjectMentorAssignments] ([MentorUserId]) 
    INCLUDE ([ProjectId], [StarterUserId], [Status], [NextSessionDate]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectMentorAssignments_StarterUserId] 
    ON [businessincubators].[ProjectMentorAssignments] ([StarterUserId]) 
    INCLUDE ([ProjectId], [MentorUserId], [Status], [NextSessionDate]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectMentorAssignments_ProjectId_Status] 
    ON [businessincubators].[ProjectMentorAssignments] ([ProjectId], [Status]) 
    INCLUDE ([MentorUserId], [StarterUserId], [NextSessionDate]);
GO

CREATE NONCLUSTERED INDEX [IX_ProjectMentorAssignments_NextSessionDate] 
    ON [businessincubators].[ProjectMentorAssignments] ([NextSessionDate]) 
    WHERE [NextSessionDate] IS NOT NULL AND [Status] = 'active';
GO