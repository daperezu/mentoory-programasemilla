-- Enable SQLCMD mode in project settings for this to work.
-- This script will be executed after the main deployment script.
-- It is useful for seeding data, etc.
-- --------------------------------------------------------------------------------------

PRINT '[Script.PostDeployment.sql] Starting';

-- All scripts now run unconditionally for consistent deployment
PRINT '[000.SeedRolesAndUsers.sql] Starting';
:r .\000.SeedRolesAndUsers.sql
PRINT '[000.SeedRolesAndUsers.sql] Finished';

PRINT '[001.SeedWebFeatures.sql] Starting';
:r .\001.SeedWebFeatures.sql
PRINT '[001.SeedWebFeatures.sql] Finished';

PRINT '[002.SeedPackagesLimits.sql] Starting';
:r .\002.SeedPackagesLimits.sql
PRINT '[002.SeedPackagesLimits.sql] Finished';

PRINT '[003.SeedKnowledgeStructures.sql] Starting';
:r .\003.SeedKnowledgeStructures.sql
PRINT '[003.SeedKnowledgeStructures.sql] Finished';

PRINT '[004.SeedKnowledgeStructuresBlocks.sql] Starting';
:r .\004.SeedKnowledgeStructuresBlocks.sql
PRINT '[004.SeedKnowledgeStructuresBlocks.sql] Finished';

PRINT '[005.SeedDashboardData.sql] Starting';
:r .\005.SeedDashboardData.sql
PRINT '[005.SeedDashboardData.sql] Finished';

PRINT '[006.SeedStarterData.sql] Starting';
:r .\006.SeedStarterData.sql
PRINT '[006.SeedStarterData.sql] Finished';

PRINT '[007.SeedAuthAccessTables.sql] Starting';
:r .\007.SeedAuthAccessTables.sql
PRINT '[007.SeedAuthAccessTables.sql] Finished';

PRINT '[008.SeedNavigationMenuItems.sql] Starting';
:r .\008.SeedNavigationMenuItems.sql
PRINT '[008.SeedNavigationMenuItems.sql] Finished';

PRINT '[009.SeedProjectStages.sql] Starting';
:r .\009.SeedProjectStages.sql
PRINT '[009.SeedProjectStages.sql] Finished';

PRINT '[010.SeedEmailTemplates.sql] Starting';
:r .\010.SeedEmailTemplates.sql
PRINT '[010.SeedEmailTemplates.sql] Finished';

PRINT '[Script.PostDeployment.sql] Finished';