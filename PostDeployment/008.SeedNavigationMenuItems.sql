-- Seed Navigation Menu Items
-- This script populates the NavigationMenuItems table with the menu structure
-- IMPORTANT: This script uses MERGE to maintain idempotency and preserve custom menu items

-- Enable IDENTITY_INSERT to use explicit Id values
SET IDENTITY_INSERT [core].[NavigationMenuItems] ON;

-- Use MERGE to safely manage navigation menu items
-- Dashboard (Root item - available to all)
MERGE [core].[NavigationMenuItems] AS target
USING (SELECT 
    1 AS Id, 'DASHBOARD' AS Code, 'Inicio' AS DisplayText, NULL AS ParentId, 1 AS SortOrder, 
    'home' AS Icon, '/AuthRedirect/RedirectToDashboard' AS Url, 0 AS IsSection, 1 AS IsActive, 
    1 AS RequiresAuthentication, 0 AS RequiresIncubator, 0 AS RequiresProject, NULL AS AllowedRoles
) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET 
        DisplayText = source.DisplayText,
        ParentId = source.ParentId,
        SortOrder = source.SortOrder,
        Icon = source.Icon,
        Url = source.Url,
        IsSection = source.IsSection,
        IsActive = source.IsActive,
        RequiresAuthentication = source.RequiresAuthentication,
        RequiresIncubator = source.RequiresIncubator,
        RequiresProject = source.RequiresProject,
        AllowedRoles = source.AllowedRoles
WHEN NOT MATCHED THEN
    INSERT (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, 
            RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
    VALUES (source.Id, source.Code, source.DisplayText, source.ParentId, source.SortOrder, 
            source.Icon, source.Url, source.IsSection, source.IsActive, 
            source.RequiresAuthentication, source.RequiresIncubator, source.RequiresProject, source.AllowedRoles);

-- Participant Section (For Starters)
MERGE [core].[NavigationMenuItems] AS target
USING (SELECT 
    50 AS Id, 'PARTICIPANT' AS Code, 'Mi Espacio' AS DisplayText, NULL AS ParentId, 5 AS SortOrder, 
    'user' AS Icon, '#' AS Url, 1 AS IsSection, 1 AS IsActive, 
    1 AS RequiresAuthentication, 0 AS RequiresIncubator, 0 AS RequiresProject, 'Starter' AS AllowedRoles
    UNION ALL
    SELECT 51, 'PARTICIPANT_DASHBOARD', 'Mi Dashboard', 50, 1, NULL, '/Participant/Dashboard', 0, 1, 1, 0, 0, 'Starter'
    UNION ALL
    SELECT 52, 'MY_PROJECTS', 'Mis Proyectos', 50, 2, NULL, '/Participant/Dashboard/MyProjects', 0, 1, 1, 0, 0, 'Starter'
    UNION ALL
    SELECT 53, 'PENDING_FORMS', 'Formularios Pendientes', 50, 3, NULL, '/Participant/Dashboard/PendingForms', 0, 1, 1, 0, 0, 'Starter'
) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET 
        DisplayText = source.DisplayText,
        ParentId = source.ParentId,
        SortOrder = source.SortOrder,
        Icon = source.Icon,
        Url = source.Url,
        IsSection = source.IsSection,
        IsActive = source.IsActive,
        RequiresAuthentication = source.RequiresAuthentication,
        RequiresIncubator = source.RequiresIncubator,
        RequiresProject = source.RequiresProject,
        AllowedRoles = source.AllowedRoles
WHEN NOT MATCHED THEN
    INSERT (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, 
            RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
    VALUES (source.Id, source.Code, source.DisplayText, source.ParentId, source.SortOrder, 
            source.Icon, source.Url, source.IsSection, source.IsActive, 
            source.RequiresAuthentication, source.RequiresIncubator, source.RequiresProject, source.AllowedRoles);

-- Incubadoras Section
MERGE [core].[NavigationMenuItems] AS target
USING (SELECT 
    100 AS Id, 'INCUBATORS' AS Code, 'Incubadoras' AS DisplayText, NULL AS ParentId, 10 AS SortOrder, 
    'briefcase' AS Icon, '#' AS Url, 1 AS IsSection, 1 AS IsActive, 
    1 AS RequiresAuthentication, 0 AS RequiresIncubator, 0 AS RequiresProject, 'Global Administrator' AS AllowedRoles
    UNION ALL
    SELECT 102, 'INCUBATORS_LIST', 'Incubadoras', 100, 1, NULL, '/BusinessIncubators/Home', 0, 1, 1, 0, 0, 'Global Administrator'
    UNION ALL
    SELECT 103, 'PROJECTS', 'Proyectos', 100, 2, NULL, '/BusinessIncubators/Projects', 0, 1, 1, 0, 0, 'Global Administrator'
) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET 
        DisplayText = source.DisplayText,
        ParentId = source.ParentId,
        SortOrder = source.SortOrder,
        Icon = source.Icon,
        Url = source.Url,
        IsSection = source.IsSection,
        IsActive = source.IsActive,
        RequiresAuthentication = source.RequiresAuthentication,
        RequiresIncubator = source.RequiresIncubator,
        RequiresProject = source.RequiresProject,
        AllowedRoles = source.AllowedRoles
WHEN NOT MATCHED THEN
    INSERT (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, 
            RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
    VALUES (source.Id, source.Code, source.DisplayText, source.ParentId, source.SortOrder, 
            source.Icon, source.Url, source.IsSection, source.IsActive, 
            source.RequiresAuthentication, source.RequiresIncubator, source.RequiresProject, source.AllowedRoles);

-- Coordinación Section (Coordinator/Administrator/Global Administrator)
MERGE [core].[NavigationMenuItems] AS target
USING (SELECT
    200 AS Id, 'COORDINATION' AS Code, 'Coordinación' AS DisplayText, NULL AS ParentId, 20 AS SortOrder,
    'users' AS Icon, '#' AS Url, 1 AS IsSection, 1 AS IsActive,
    1 AS RequiresAuthentication, 1 AS RequiresIncubator, 0 AS RequiresProject, 'Coordinator,Administrator,Global Administrator' AS AllowedRoles
    UNION ALL
    SELECT 203, 'PARTICIPANTS', 'Participantes', 200, 2, NULL, '/Coordination/Participant', 0, 1, 1, 1, 1, 'Coordinator,Administrator,Global Administrator'
    UNION ALL
    SELECT 205, 'FORM_REVIEW', 'Revisión de Formularios', 200, 4, NULL, '/Coordination/FormReview', 0, 1, 1, 1, 1, 'Coordinator,Administrator,Global Administrator'
    UNION ALL
    SELECT 206, 'REPORTS', 'Reportes', 200, 5, NULL, '/Coordination/Reports', 0, 1, 1, 1, 1, 'Coordinator,Administrator,Global Administrator'
    UNION ALL
    SELECT 207, 'USER_MANAGEMENT', 'Gestión de Usuarios', 200, 6, NULL, '/Coordination/UserManagement', 0, 1, 1, 0, 0, 'Administrator,Global Administrator'
    UNION ALL
    SELECT 208, 'EMAIL_TEMPLATES', 'Plantillas de Email', 200, 7, NULL, '/Coordination/EmailTemplate', 0, 1, 1, 0, 0, 'Administrator,Global Administrator'
    UNION ALL
    SELECT 209, 'AUDIT_LOGS', 'Registros de Auditoría', 200, 8, NULL, '/Coordination/Audit', 0, 1, 1, 0, 0, 'Administrator,Global Administrator'
) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET 
        DisplayText = source.DisplayText,
        ParentId = source.ParentId,
        SortOrder = source.SortOrder,
        Icon = source.Icon,
        Url = source.Url,
        IsSection = source.IsSection,
        IsActive = source.IsActive,
        RequiresAuthentication = source.RequiresAuthentication,
        RequiresIncubator = source.RequiresIncubator,
        RequiresProject = source.RequiresProject,
        AllowedRoles = source.AllowedRoles
WHEN NOT MATCHED THEN
    INSERT (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, 
            RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
    VALUES (source.Id, source.Code, source.DisplayText, source.ParentId, source.SortOrder, 
            source.Icon, source.Url, source.IsSection, source.IsActive, 
            source.RequiresAuthentication, source.RequiresIncubator, source.RequiresProject, source.AllowedRoles);

-- Knowledge Structure Section
MERGE [core].[NavigationMenuItems] AS target
USING (SELECT 
    300 AS Id, 'KNOWLEDGE' AS Code, 'Estructura de Conocimiento' AS DisplayText, NULL AS ParentId, 30 AS SortOrder, 
    'book' AS Icon, '#' AS Url, 1 AS IsSection, 1 AS IsActive, 
    1 AS RequiresAuthentication, 0 AS RequiresIncubator, 0 AS RequiresProject, 'Global Administrator' AS AllowedRoles
    UNION ALL
    SELECT 302, 'STRUCTURES', 'Estructuras', 300, 1, NULL, '/KnowledgeStructure/KnowledgeStructure', 0, 1, 1, 0, 0, 'Global Administrator'
    UNION ALL
    SELECT 303, 'MODULES', 'Módulos', 300, 2, NULL, '/KnowledgeStructure/Modules', 0, 1, 1, 0, 0, 'Global Administrator'
    UNION ALL
    SELECT 304, 'TOPICS', 'Temas', 300, 3, NULL, '/KnowledgeStructure/Topics', 0, 1, 1, 0, 0, 'Global Administrator'
    UNION ALL
    SELECT 305, 'SUBJECTS', 'Materias', 300, 4, NULL, '/KnowledgeStructure/Subjects', 0, 1, 1, 0, 0, 'Global Administrator'
    UNION ALL
    SELECT 306, 'PROJECT_KNOWLEDGE', 'Estructura del Proyecto', 300, 5, NULL, '/BusinessIncubators/{incubatorId}/Projects/{projectId}/KnowledgeStructure', 0, 1, 1, 1, 1, 'Administrator,Global Administrator'
) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET 
        DisplayText = source.DisplayText,
        ParentId = source.ParentId,
        SortOrder = source.SortOrder,
        Icon = source.Icon,
        Url = source.Url,
        IsSection = source.IsSection,
        IsActive = source.IsActive,
        RequiresAuthentication = source.RequiresAuthentication,
        RequiresIncubator = source.RequiresIncubator,
        RequiresProject = source.RequiresProject,
        AllowedRoles = source.AllowedRoles
WHEN NOT MATCHED THEN
    INSERT (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, 
            RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
    VALUES (source.Id, source.Code, source.DisplayText, source.ParentId, source.SortOrder, 
            source.Icon, source.Url, source.IsSection, source.IsActive, 
            source.RequiresAuthentication, source.RequiresIncubator, source.RequiresProject, source.AllowedRoles);

-- Diagnostics Section  
MERGE [core].[NavigationMenuItems] AS target
USING (SELECT 
    400 AS Id, 'DIAGNOSTICS' AS Code, 'Pruebas de diagnóstico' AS DisplayText, NULL AS ParentId, 40 AS SortOrder, 
    'clipboard' AS Icon, '#' AS Url, 1 AS IsSection, 1 AS IsActive, 
    1 AS RequiresAuthentication, 0 AS RequiresIncubator, 0 AS RequiresProject, 'Global Administrator' AS AllowedRoles
    UNION ALL
    SELECT 401, 'FORMS_MGMT', 'Gestión de formularios', 400, 1, NULL, '/Diagnostics/Forms/List', 0, 1, 1, 0, 0, 'Global Administrator'
    UNION ALL
    SELECT 402, 'QUESTIONS_MGMT', 'Gestión de preguntas', 400, 2, 'help-circle', '/Diagnostics/Questions/List', 0, 1, 1, 0, 0, 'Global Administrator'
    UNION ALL
    SELECT 403, 'BULK_LOAD', 'Carga masiva', 400, 3, 'upload', '/Diagnostics/Forms/LoadCSV', 0, 1, 1, 0, 0, 'Global Administrator'
) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET 
        DisplayText = source.DisplayText,
        ParentId = source.ParentId,
        SortOrder = source.SortOrder,
        Icon = source.Icon,
        Url = source.Url,
        IsSection = source.IsSection,
        IsActive = source.IsActive,
        RequiresAuthentication = source.RequiresAuthentication,
        RequiresIncubator = source.RequiresIncubator,
        RequiresProject = source.RequiresProject,
        AllowedRoles = source.AllowedRoles
WHEN NOT MATCHED THEN
    INSERT (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, 
            RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
    VALUES (source.Id, source.Code, source.DisplayText, source.ParentId, source.SortOrder, 
            source.Icon, source.Url, source.IsSection, source.IsActive, 
            source.RequiresAuthentication, source.RequiresIncubator, source.RequiresProject, source.AllowedRoles);

-- Administration Section (Global Administrator only)
MERGE [core].[NavigationMenuItems] AS target
USING (SELECT 
    500 AS Id, 'ADMINISTRATION' AS Code, 'Administración del Sistema' AS DisplayText, NULL AS ParentId, 50 AS SortOrder, 
    'settings' AS Icon, '#' AS Url, 1 AS IsSection, 1 AS IsActive, 
    1 AS RequiresAuthentication, 0 AS RequiresIncubator, 0 AS RequiresProject, 'Global Administrator' AS AllowedRoles
) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET 
        DisplayText = source.DisplayText,
        ParentId = source.ParentId,
        SortOrder = source.SortOrder,
        Icon = source.Icon,
        Url = source.Url,
        IsSection = source.IsSection,
        IsActive = source.IsActive,
        RequiresAuthentication = source.RequiresAuthentication,
        RequiresIncubator = source.RequiresIncubator,
        RequiresProject = source.RequiresProject,
        AllowedRoles = source.AllowedRoles
WHEN NOT MATCHED THEN
    INSERT (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, 
            RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
    VALUES (source.Id, source.Code, source.DisplayText, source.ParentId, source.SortOrder, 
            source.Icon, source.Url, source.IsSection, source.IsActive, 
            source.RequiresAuthentication, source.RequiresIncubator, source.RequiresProject, source.AllowedRoles);

-- Mentoring Section (for future use)
MERGE [core].[NavigationMenuItems] AS target
USING (SELECT 
    600 AS Id, 'MENTORING' AS Code, 'Mentoría' AS DisplayText, NULL AS ParentId, 60 AS SortOrder, 
    'award' AS Icon, '#' AS Url, 1 AS IsSection, 0 AS IsActive, 
    1 AS RequiresAuthentication, 0 AS RequiresIncubator, 0 AS RequiresProject, 'Mentor,Coordinator' AS AllowedRoles
    UNION ALL
    SELECT 601, 'MENTOR_DASHBOARD', 'Panel Mentor', 600, 1, NULL, '/Mentoring/Mentor', 0, 0, 1, 0, 0, 'Mentor,Coordinator'
    UNION ALL
    SELECT 602, 'SESSIONS', 'Sesiones', 600, 2, NULL, '/Mentoring/Sessions', 0, 0, 1, 0, 0, 'Mentor,Coordinator'
) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET 
        DisplayText = source.DisplayText,
        ParentId = source.ParentId,
        SortOrder = source.SortOrder,
        Icon = source.Icon,
        Url = source.Url,
        IsSection = source.IsSection,
        IsActive = source.IsActive,
        RequiresAuthentication = source.RequiresAuthentication,
        RequiresIncubator = source.RequiresIncubator,
        RequiresProject = source.RequiresProject,
        AllowedRoles = source.AllowedRoles
WHEN NOT MATCHED THEN
    INSERT (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, 
            RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
    VALUES (source.Id, source.Code, source.DisplayText, source.ParentId, source.SortOrder, 
            source.Icon, source.Url, source.IsSection, source.IsActive, 
            source.RequiresAuthentication, source.RequiresIncubator, source.RequiresProject, source.AllowedRoles);

-- Disable IDENTITY_INSERT after all MERGE operations
SET IDENTITY_INSERT [core].[NavigationMenuItems] OFF;
