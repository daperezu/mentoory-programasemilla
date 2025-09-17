CREATE TABLE [businessincubators].[ProjectInterests]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProjectId] BIGINT NOT NULL,
    [ObserverUserId] NVARCHAR(450) NULL, -- NULL for anonymous views
    [ObserverSessionId] NVARCHAR(100) NULL, -- For anonymous tracking
    [InterestType] VARCHAR(20) NOT NULL, -- 'View', 'Contact', 'Apply'
    [UserAgent] NVARCHAR(500) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [ReferrerUrl] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    
    -- Geolocation at time of interest
    [ObserverLatitude] DECIMAL(10, 8) NULL,
    [ObserverLongitude] DECIMAL(11, 8) NULL,
    [Distance] DECIMAL(10, 2) NULL, -- Distance in km
    
    CONSTRAINT [CHK_ProjectInterests_InterestType] CHECK ([InterestType] IN ('View', 'Contact', 'Apply')),
    CONSTRAINT [FK_ProjectInterests_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [businessincubators].[Projects] ([Id])
);