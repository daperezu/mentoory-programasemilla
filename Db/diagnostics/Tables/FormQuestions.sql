CREATE TABLE [diagnostics].FormQuestions (
    FormId BIGINT NOT NULL,
    QuestionId BIGINT NOT NULL,
    TopicId BIGINT NULL,
    BlockId BIGINT NOT NULL,
    [Order] INT NOT NULL,

    PRIMARY KEY (FormId, QuestionId),
    FOREIGN KEY (FormId) REFERENCES [diagnostics].Forms(Id),
    FOREIGN KEY (QuestionId) REFERENCES [diagnostics].Questions(Id),
    FOREIGN KEY (BlockId) REFERENCES [diagnostics].Blocks(Id),
);
