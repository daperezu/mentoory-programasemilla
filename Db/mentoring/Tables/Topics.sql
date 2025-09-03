CREATE TABLE [mentoring].[Topics]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ModuleId] BIGINT NOT NULL,
    [SourceTopicId] BIGINT NULL,
    [Title] NVARCHAR(255) NOT NULL
);
