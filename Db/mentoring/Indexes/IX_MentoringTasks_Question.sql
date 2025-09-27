CREATE NONCLUSTERED INDEX IX_Tasks_Question
    ON [mentoring].[Tasks] ([QuestionId], [ForAnswer], [Status]);
