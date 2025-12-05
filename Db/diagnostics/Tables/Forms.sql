CREATE TABLE [diagnostics].Forms (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    SourceKnowledgeStructureId BIGINT NULL,
);