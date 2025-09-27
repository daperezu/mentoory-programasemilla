CREATE TABLE [diagnostics].AnswerOptions (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    QuestionId BIGINT NOT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    Score INT NOT NULL,
    Foda CHAR(1) NOT NULL,
    FodaExplanation NVARCHAR(MAX) NOT NULL,
    Odsr CHAR(1) NOT NULL,
    OdsrExplanation NVARCHAR(MAX) NOT NULL,
    [Order] INT NOT NULL,
    [FollowUpQuestionText] NVARCHAR(MAX) NULL,
    FOREIGN KEY (QuestionId) REFERENCES [diagnostics].Questions(Id),
);