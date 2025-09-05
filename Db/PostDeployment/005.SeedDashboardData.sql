-- Seed data for Dashboard Widgets and Templates
-- This script seeds the initial dashboard configuration for all roles
-- IMPORTANT: Uses MERGE for idempotency - safe to run multiple times

-- Dashboard Widgets - Use MERGE for idempotent inserts/updates
MERGE [core].[DashboardWidgets] AS target
USING (
    SELECT * FROM (VALUES
        -- Shared widgets (available to all roles)
        ('progress_overview', 'Progreso General', 'Muestra el progreso general del usuario', 'kpi', 'Widgets/ProgressOverview', 'bi-graph-up', 
         '{"showPercentage": true, "showChart": true}', '["Starter","Mentor","Facilitator","Guide","Coordinator","Administrator","Global Administrator"]', 'small', 'medium'),
        
        ('active_tasks', 'Tareas Activas', 'Lista de tareas pendientes y activas', 'list', 'Widgets/ActiveTasks', 'bi-list-task', 
         '{"maxItems": 5, "showDueDate": true}', '["Starter","Mentor","Facilitator","Guide","Coordinator","Administrator","Global Administrator"]', 'medium', 'large'),
        
        ('notifications', 'Notificaciones', 'Centro de notificaciones', 'feed', 'Widgets/NotificationCenter', 'bi-bell', 
         '{"maxItems": 10, "groupByType": true}', '["Starter","Mentor","Facilitator","Guide","Coordinator","Administrator","Global Administrator","Liaison"]', 'small', 'medium'),
        
        ('quick_actions', 'Acciones Rápidas', 'Accesos directos a funciones comunes', 'list', 'Widgets/QuickActions', 'bi-lightning', 
         '{"columns": 2}', '["Starter","Mentor","Facilitator","Guide","Coordinator","Administrator","Global Administrator","Liaison"]', 'small', 'medium'),
        
        -- Starter-specific widgets
        ('form_status', 'Estado de Formularios', 'Resumen del estado de formularios', 'kpi', 'Widgets/FormStatus', 'bi-file-text', 
         '{"showPending": true, "showCompleted": true}', '["Starter"]', 'small', 'medium'),
        
        ('learning_progress', 'Progreso de Aprendizaje', 'Visualización del progreso en el programa', 'chart', 'Widgets/LearningProgress', 'bi-mortarboard', 
         '{"chartType": "timeline"}', '["Starter"]', 'medium', 'large'),
        
        ('mentor_contact', 'Mi Mentor', 'Información de contacto del mentor asignado', 'kpi', 'Widgets/MentorContact', 'bi-person-check', 
         '{"showAvatar": true, "showSchedule": true}', '["Starter"]', 'small', 'small'),
        
        -- Administrator widgets
        ('system_health', 'Salud del Sistema', 'Monitoreo del estado del sistema', 'kpi', 'Widgets/SystemHealth', 'bi-heart-pulse', 
         '{"showMetrics": true, "alertThreshold": 80}', '["Administrator","Global Administrator"]', 'medium', 'large'),
        
        ('user_statistics', 'Estadísticas de Usuarios', 'Resumen de usuarios activos', 'chart', 'Widgets/UserStatistics', 'bi-people', 
         '{"chartType": "donut", "showTrends": true}', '["Administrator","Global Administrator","Coordinator"]', 'medium', 'large'),
        
        ('approval_queue', 'Cola de Aprobaciones', 'Elementos pendientes de aprobación', 'list', 'Widgets/ApprovalQueue', 'bi-check-square', 
         '{"maxItems": 10, "groupByType": true}', '["Administrator","Global Administrator","Coordinator"]', 'medium', 'large'),
        
        -- Mentor widgets
        ('assigned_participants', 'Participantes Asignados', 'Lista de participantes bajo mentoría', 'list', 'Widgets/AssignedParticipants', 'bi-people-fill', 
         '{"showProgress": true, "showLastActivity": true}', '["Mentor","Facilitator","Guide"]', 'medium', 'large'),
        
        ('session_calendar', 'Calendario de Sesiones', 'Próximas sesiones programadas', 'calendar', 'Widgets/SessionCalendar', 'bi-calendar-event', 
         '{"viewType": "week", "showReminders": true}', '["Mentor","Facilitator","Guide","Coordinator"]', 'medium', 'full'),
        
        ('feedback_tracker', 'Seguimiento de Retroalimentación', 'Estado de retroalimentación pendiente', 'list', 'Widgets/FeedbackTracker', 'bi-chat-dots', 
         '{"showPriority": true}', '["Mentor","Facilitator","Guide"]', 'small', 'medium')
    ) AS v([Name], [DisplayName], [Description], [Type], [Component], [IconClass], [DefaultConfig], [Roles], [MinSize], [MaxSize])
) AS source
ON target.[Name] = source.[Name]
WHEN MATCHED THEN
    UPDATE SET 
        [DisplayName] = source.[DisplayName],
        [Description] = source.[Description],
        [Type] = source.[Type],
        [Component] = source.[Component],
        [IconClass] = source.[IconClass],
        [DefaultConfig] = source.[DefaultConfig],
        [Roles] = source.[Roles],
        [MinSize] = source.[MinSize],
        [MaxSize] = source.[MaxSize]
WHEN NOT MATCHED THEN
    INSERT ([Name], [DisplayName], [Description], [Type], [Component], [IconClass], [DefaultConfig], [Roles], [MinSize], [MaxSize])
    VALUES (source.[Name], source.[DisplayName], source.[Description], source.[Type], source.[Component], 
            source.[IconClass], source.[DefaultConfig], source.[Roles], source.[MinSize], source.[MaxSize]);

-- Role Dashboard Templates - Use MERGE for idempotent inserts/updates
MERGE [core].[RoleDashboardTemplates] AS target
USING (
    SELECT 
        r.Name AS [Role],
        r.Name AS [RoleName],
        CASE r.Name
            WHEN 'Starter' THEN '[
                {"widgetName": "progress_overview", "row": 0, "col": 0, "width": 3, "height": 1},
                {"widgetName": "form_status", "row": 0, "col": 3, "width": 3, "height": 1},
                {"widgetName": "active_tasks", "row": 0, "col": 6, "width": 6, "height": 2},
                {"widgetName": "learning_progress", "row": 1, "col": 0, "width": 6, "height": 2},
                {"widgetName": "notifications", "row": 2, "col": 6, "width": 3, "height": 2},
                {"widgetName": "mentor_contact", "row": 2, "col": 9, "width": 3, "height": 1},
                {"widgetName": "quick_actions", "row": 3, "col": 9, "width": 3, "height": 1}
            ]'
            WHEN 'Administrator' THEN '[
                {"widgetName": "system_health", "row": 0, "col": 0, "width": 6, "height": 1},
                {"widgetName": "user_statistics", "row": 0, "col": 6, "width": 6, "height": 1},
                {"widgetName": "approval_queue", "row": 1, "col": 0, "width": 8, "height": 2},
                {"widgetName": "notifications", "row": 1, "col": 8, "width": 4, "height": 1},
                {"widgetName": "quick_actions", "row": 2, "col": 8, "width": 4, "height": 1}
            ]'
            WHEN 'Global Administrator' THEN '[
                {"widgetName": "system_health", "row": 0, "col": 0, "width": 6, "height": 1},
                {"widgetName": "user_statistics", "row": 0, "col": 6, "width": 6, "height": 1},
                {"widgetName": "approval_queue", "row": 1, "col": 0, "width": 8, "height": 2},
                {"widgetName": "notifications", "row": 1, "col": 8, "width": 4, "height": 1},
                {"widgetName": "quick_actions", "row": 2, "col": 8, "width": 4, "height": 1}
            ]'
            WHEN 'Mentor' THEN '[
                {"widgetName": "assigned_participants", "row": 0, "col": 0, "width": 6, "height": 2},
                {"widgetName": "session_calendar", "row": 0, "col": 6, "width": 6, "height": 2},
                {"widgetName": "feedback_tracker", "row": 2, "col": 0, "width": 4, "height": 1},
                {"widgetName": "active_tasks", "row": 2, "col": 4, "width": 4, "height": 1},
                {"widgetName": "notifications", "row": 2, "col": 8, "width": 4, "height": 1}
            ]'
            WHEN 'Facilitator' THEN '[
                {"widgetName": "assigned_participants", "row": 0, "col": 0, "width": 6, "height": 2},
                {"widgetName": "session_calendar", "row": 0, "col": 6, "width": 6, "height": 2},
                {"widgetName": "feedback_tracker", "row": 2, "col": 0, "width": 4, "height": 1},
                {"widgetName": "active_tasks", "row": 2, "col": 4, "width": 4, "height": 1},
                {"widgetName": "notifications", "row": 2, "col": 8, "width": 4, "height": 1}
            ]'
            WHEN 'Guide' THEN '[
                {"widgetName": "assigned_participants", "row": 0, "col": 0, "width": 6, "height": 2},
                {"widgetName": "session_calendar", "row": 0, "col": 6, "width": 6, "height": 2},
                {"widgetName": "feedback_tracker", "row": 2, "col": 0, "width": 4, "height": 1},
                {"widgetName": "active_tasks", "row": 2, "col": 4, "width": 4, "height": 1},
                {"widgetName": "notifications", "row": 2, "col": 8, "width": 4, "height": 1}
            ]'
            WHEN 'Coordinator' THEN '[
                {"widgetName": "user_statistics", "row": 0, "col": 0, "width": 6, "height": 1},
                {"widgetName": "progress_overview", "row": 0, "col": 6, "width": 6, "height": 1},
                {"widgetName": "session_calendar", "row": 1, "col": 0, "width": 8, "height": 2},
                {"widgetName": "approval_queue", "row": 1, "col": 8, "width": 4, "height": 1},
                {"widgetName": "notifications", "row": 2, "col": 8, "width": 4, "height": 1}
            ]'
            ELSE '[ -- Liaison
                {"widgetName": "active_tasks", "row": 0, "col": 0, "width": 6, "height": 2},
                {"widgetName": "notifications", "row": 0, "col": 6, "width": 6, "height": 1},
                {"widgetName": "quick_actions", "row": 1, "col": 6, "width": 6, "height": 1}
            ]'
        END AS [DefaultLayout],
        'light' AS [DefaultTheme],
        'es' AS [DefaultLanguage],
        300 AS [DefaultRefreshInterval],
        CASE r.Name
            WHEN 'Starter' THEN 'progress_overview,form_status,active_tasks,learning_progress,notifications,mentor_contact,quick_actions'
            WHEN 'Administrator' THEN 'system_health,user_statistics,approval_queue,notifications,quick_actions'
            WHEN 'Global Administrator' THEN 'system_health,user_statistics,approval_queue,notifications,quick_actions'
            WHEN 'Mentor' THEN 'assigned_participants,session_calendar,feedback_tracker,active_tasks,notifications'
            WHEN 'Facilitator' THEN 'assigned_participants,session_calendar,feedback_tracker,active_tasks,notifications'
            WHEN 'Guide' THEN 'assigned_participants,session_calendar,feedback_tracker,active_tasks,notifications'
            WHEN 'Coordinator' THEN 'user_statistics,progress_overview,session_calendar,approval_queue,notifications'
            ELSE 'active_tasks,notifications,quick_actions' -- Liaison
        END AS [WidgetCodes],
        1 AS [IsActive],
        GETUTCDATE() AS [CreatedDate]
    FROM [dbo].[AspNetRoles] r
    WHERE r.Name IN ('Starter', 'Administrator', 'Global Administrator', 'Mentor', 'Facilitator', 'Guide', 'Coordinator', 'Liaison')
) AS source
ON target.[Role] = source.[Role] AND target.[IsActive] = 1
WHEN MATCHED THEN
    UPDATE SET
        [RoleName] = source.[RoleName],
        [DefaultLayout] = source.[DefaultLayout],
        [DefaultTheme] = source.[DefaultTheme],
        [DefaultLanguage] = source.[DefaultLanguage],
        [DefaultRefreshInterval] = source.[DefaultRefreshInterval],
        [WidgetCodes] = source.[WidgetCodes]
WHEN NOT MATCHED THEN
    INSERT ([Role], [RoleName], [DefaultLayout], [DefaultTheme], [DefaultLanguage], [DefaultRefreshInterval], [WidgetCodes], [IsActive], [CreatedDate])
    VALUES (source.[Role], source.[RoleName], source.[DefaultLayout], source.[DefaultTheme], source.[DefaultLanguage], 
            source.[DefaultRefreshInterval], source.[WidgetCodes], source.[IsActive], source.[CreatedDate]);

-- Create indexes if they don't exist (idempotent index creation)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DashboardWidgets_Type_IsActive')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DashboardWidgets_Type_IsActive]
        ON [core].[DashboardWidgets] ([Type], [IsActive]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RoleDashboardTemplates_Role_IsActive')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RoleDashboardTemplates_Role_IsActive]
        ON [core].[RoleDashboardTemplates] ([Role], [IsActive]);
END;

PRINT 'Dashboard seed data completed successfully (using MERGE for idempotency)';