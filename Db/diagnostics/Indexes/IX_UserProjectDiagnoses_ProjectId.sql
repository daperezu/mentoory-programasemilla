CREATE NONCLUSTERED INDEX [IX_UserProjectDiagnoses_ProjectId] 
ON [diagnostics].[UserProjectDiagnoses]([ProjectId] ASC)
INCLUDE ([UserId], [Status]);
GO