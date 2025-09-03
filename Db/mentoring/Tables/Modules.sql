CREATE TABLE [mentoring].[Modules]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProgramId] BIGINT NOT NULL,
    [SourceModuleId] BIGINT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Order] INT NOT NULL,

    CONSTRAINT FK_MentoringModules_MentoringPrograms FOREIGN KEY ([ProgramId])
        REFERENCES [mentoring].[Programs] ([Id])
        ON DELETE CASCADE
);
