CREATE TABLE [mentoring].[Programs]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] BIGINT NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL
);
