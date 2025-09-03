CREATE TABLE [businessincubators].[ProjectFormFeedback]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [ExternalId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [ReviewId] BIGINT NOT NULL,
    [BlockId] BIGINT NULL,
    [QuestionId] BIGINT NULL,
    [FeedbackText] NVARCHAR(MAX) NOT NULL,
    [FeedbackType] INT NOT NULL, -- 0=Info, 1=Warning, 2=Error
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(450) NOT NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(450) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2 NULL,
    [DeletedBy] NVARCHAR(450) NULL,
    CONSTRAINT [PK_ProjectFormFeedback] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProjectFormFeedback_ProjectFormReviews] FOREIGN KEY ([ReviewId]) 
        REFERENCES [businessincubators].[ProjectFormReviews]([Id])
);