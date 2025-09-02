CREATE NONCLUSTERED INDEX [IX_UserProjectDiagnoses_Status] 
ON [diagnostics].[UserProjectDiagnoses]([Status] ASC)
INCLUDE ([ProjectId], [UserId]);
GO