-- Seed Navigation Menu Items
-- This script populates the NavigationMenuItems table with the menu structure

-- Clear existing data (only if table exists)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NavigationMenuItems' AND schema_id = SCHEMA_ID('core'))
BEGIN
    DELETE FROM [core].[NavigationMenuItems];
END

SET IDENTITY_INSERT [core].[NavigationMenuItems] ON;

-- Dashboard (Root item - available to all)
INSERT INTO [core].[NavigationMenuItems] 
    (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
VALUES 
    (1, 'DASHBOARD', 'Inicio', NULL, 1, 'home', '/AuthRedirect/RedirectToDashboard', 0, 1, 1, 0, 0, NULL);

-- Participant Section (For Starters)
INSERT INTO [core].[NavigationMenuItems] 
    (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
VALUES 
    (50, 'PARTICIPANT', 'Mi Espacio', NULL, 5, 'user', '#', 1, 1, 1, 0, 0, 'Starter'),
    (51, 'PARTICIPANT_DASHBOARD', 'Mi Dashboard', 50, 1, NULL, '/Participant/Dashboard', 0, 1, 1, 0, 0, 'Starter'),
    (52, 'MY_PROJECTS', 'Mis Proyectos', 50, 2, NULL, '/Participant/Dashboard/MyProjects', 0, 1, 1, 0, 0, 'Starter'),
    (53, 'PENDING_FORMS', 'Formularios Pendientes', 50, 3, NULL, '/Participant/Dashboard/PendingForms', 0, 1, 1, 0, 0, 'Starter');

-- Incubadoras Section
INSERT INTO [core].[NavigationMenuItems] 
    (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
VALUES 
    (100, 'INCUBATORS', 'Incubadoras', NULL, 10, 'briefcase', '#', 1, 1, 1, 0, 0, 'Global Administrator'),
    (102, 'INCUBATORS_LIST', 'Incubadoras', 100, 1, NULL, '/BusinessIncubators/Home', 0, 1, 1, 0, 0, 'Global Administrator'),
    (103, 'PROJECTS', 'Proyectos', 100, 2, NULL, '/BusinessIncubators/Projects', 0, 1, 1, 0, 0, 'Global Administrator');

-- Coordinación Section (Coordinator/Administrator/Global Administrator)
INSERT INTO [core].[NavigationMenuItems] 
    (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
VALUES 
    (200, 'COORDINATION', 'Coordinación', NULL, 20, 'users', '#', 1, 1, 1, 1, 1, 'Coordinator,Administrator,Global Administrator'),
    (203, 'PARTICIPANTS', 'Participantes', 200, 2, NULL, '/Coordination/Participant', 0, 1, 1, 1, 1, 'Coordinator,Administrator,Global Administrator'),
    (205, 'FORM_REVIEW', 'Revisión de Formularios', 200, 4, NULL, '/Coordination/FormReview', 0, 1, 1, 1, 1, 'Coordinator,Administrator,Global Administrator'),
    (206, 'REPORTS', 'Reportes', 200, 5, NULL, '/Coordination/Reports', 0, 1, 1, 1, 1, 'Coordinator,Administrator,Global Administrator'),
    (207, 'USER_MANAGEMENT', 'Gestión de Usuarios', 200, 6, NULL, '/Coordination/UserManagement', 0, 1, 1, 0, 0, 'Administrator,Global Administrator'),
    (208, 'EMAIL_TEMPLATES', 'Plantillas de Email', 200, 7, NULL, '/Coordination/EmailTemplate', 0, 1, 1, 0, 0, 'Administrator,Global Administrator'),
    (209, 'AUDIT_LOGS', 'Registros de Auditoría', 200, 8, NULL, '/Coordination/Audit', 0, 1, 1, 0, 0, 'Administrator,Global Administrator');

-- Knowledge Structure Section
INSERT INTO [core].[NavigationMenuItems] 
    (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
VALUES 
    (300, 'KNOWLEDGE', 'Estructura de Conocimiento', NULL, 30, 'book', '#', 1, 1, 1, 0, 0, 'Global Administrator'),
    (302, 'STRUCTURES', 'Estructuras', 300, 1, NULL, '/KnowledgeStructure/KnowledgeStructure', 0, 1, 1, 0, 0, 'Global Administrator'),
    (303, 'MODULES', 'Módulos', 300, 2, NULL, '/KnowledgeStructure/Modules', 0, 1, 1, 0, 0, 'Global Administrator'),
    (304, 'TOPICS', 'Temas', 300, 3, NULL, '/KnowledgeStructure/Topics', 0, 1, 1, 0, 0, 'Global Administrator'),
    (305, 'SUBJECTS', 'Materias', 300, 4, NULL, '/KnowledgeStructure/Subjects', 0, 1, 1, 0, 0, 'Global Administrator');

-- Diagnostics Section  
INSERT INTO [core].[NavigationMenuItems] 
    (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
VALUES 
    (400, 'DIAGNOSTICS', 'Pruebas de diagnóstico', NULL, 40, 'clipboard', '#', 1, 1, 1, 0, 0, 'Global Administrator'),
    (401, 'FORMS_MGMT', 'Gestión de formularios', 400, 1, NULL, '/Diagnostics/Forms/List', 0, 1, 1, 0, 0, 'Global Administrator'),
    (402, 'QUESTIONS_MGMT', 'Gestión de preguntas', 400, 2, 'help-circle', '/Diagnostics/Questions/List', 0, 1, 1, 0, 0, 'Global Administrator'),
    (403, 'BULK_LOAD', 'Carga masiva', 400, 3, 'upload', '/Diagnostics/Forms/LoadCSV', 0, 1, 1, 0, 0, 'Global Administrator');

-- Administration Section (Global Administrator only)
INSERT INTO [core].[NavigationMenuItems] 
    (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
VALUES 
    (500, 'ADMINISTRATION', 'Administración del Sistema', NULL, 50, 'settings', '#', 1, 1, 1, 0, 0, 'Global Administrator'),
    (501, 'PERMISSIONS', 'Permisos', 500, 1, NULL, '/Permissions/ProtectedResources', 0, 1, 1, 0, 0, 'Global Administrator');

-- Mentoring Section (for future use)
INSERT INTO [core].[NavigationMenuItems] 
    (Id, Code, DisplayText, ParentId, SortOrder, Icon, Url, IsSection, IsActive, RequiresAuthentication, RequiresIncubator, RequiresProject, AllowedRoles)
VALUES 
    (600, 'MENTORING', 'Mentoría', NULL, 60, 'award', '#', 1, 0, 1, 0, 0, 'Mentor,Coordinator'),
    (601, 'MENTOR_DASHBOARD', 'Panel Mentor', 600, 1, NULL, '/Mentoring/Mentor', 0, 0, 1, 0, 0, 'Mentor,Coordinator'),
    (602, 'SESSIONS', 'Sesiones', 600, 2, NULL, '/Mentoring/Sessions', 0, 0, 1, 0, 0, 'Mentor,Coordinator');

SET IDENTITY_INSERT [core].[NavigationMenuItems] OFF;
