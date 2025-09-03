CREATE TABLE [diagnostics].Questions (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Text] NVARCHAR(MAX) NOT NULL,
    AnswerType INT NOT NULL,
    AppliesToPhase INT NOT NULL,
    IsUsedForMentoringPlan BIT NOT NULL,
    IsUsedForDiagnosis BIT NOT NULL
);