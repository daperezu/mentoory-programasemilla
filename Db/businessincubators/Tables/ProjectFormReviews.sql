CREATE TABLE [businessincubators].[ProjectFormReviews]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [ExternalId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [SubmissionId] BIGINT NOT NULL,
    [ReviewerId] NVARCHAR(450) NOT NULL,
    [Status] INT NOT NULL, -- 0=Pending, 1=Approved, 2=ChangesRequested, 3=Flagged
    [ReviewedAt] DATETIME2 NOT NULL,
    [GeneralComments] NVARCHAR(MAX) NULL,
    [NewDeadline] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(450) NOT NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2 NULL,
    [DeletedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_ProjectFormReviews] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProjectFormReviews_ProjectFormSubmissions] FOREIGN KEY ([SubmissionId]) 
        REFERENCES [businessincubators].[ProjectFormSubmissions]([Id])
);