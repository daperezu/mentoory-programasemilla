CREATE TABLE [core].[NavigationMenuItems] (
    -- Identity
    Id BIGINT IDENTITY(1,1) NOT NULL,
    Code NVARCHAR(50) NOT NULL,             -- Unique code (e.g., 'PROJECT_MANAGEMENT')
    DisplayText NVARCHAR(100) NOT NULL,     -- User-facing Spanish text
    
    -- Hierarchy
    ParentId BIGINT NULL,                   -- Self-referencing for tree structure
    SortOrder INT NOT NULL DEFAULT 0,       -- Display order within parent
    Level INT NOT NULL DEFAULT 0,           -- Depth level (0=root, 1=first level, etc.)
    
    -- Presentation
    Icon NVARCHAR(50) NULL,                 -- Bootstrap icon class (e.g., 'bi-folder')
    CssClass NVARCHAR(100) NULL,           -- Additional CSS classes
    
    -- Navigation
    Url NVARCHAR(500) NULL,                -- Full URL path (e.g., '/BusinessIncubators/Projects')
    
    -- Display Control
    IsSection BIT NOT NULL DEFAULT 0,      -- True = header/divider, False = clickable
    IsActive BIT NOT NULL DEFAULT 1,       -- Enable/disable menu item
    
    -- Context Requirements
    RequiresAuthentication BIT NOT NULL DEFAULT 1,
    RequiresIncubator BIT NOT NULL DEFAULT 0,
    RequiresProject BIT NOT NULL DEFAULT 0,
    
    -- Authorization (Simple)
    AllowedRoles NVARCHAR(500) NULL,       -- Comma-separated: 'Starter,Mentor,Coordinator'
    
    -- Constraints
    CONSTRAINT PK_NavigationMenuItems PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_NavigationMenuItems_Parent FOREIGN KEY (ParentId) 
        REFERENCES [core].[NavigationMenuItems](Id),
    CONSTRAINT UQ_NavigationMenuItems_Code UNIQUE (Code)
);
GO

-- Indexes for performance
CREATE NONCLUSTERED INDEX IX_NavigationMenuItems_ParentId 
    ON [core].[NavigationMenuItems] (ParentId) 
    INCLUDE (SortOrder, IsActive);
GO

CREATE NONCLUSTERED INDEX IX_NavigationMenuItems_IsActive 
    ON [core].[NavigationMenuItems] (IsActive) 
    WHERE IsActive = 1;
GO