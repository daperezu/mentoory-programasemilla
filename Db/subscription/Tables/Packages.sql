CREATE TABLE [subscription].[Packages]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(100) NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedAt] DATETIME2 NULL
);