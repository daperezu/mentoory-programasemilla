-- ==========================================================================================
-- Post-Deployment Script for Seeding the web features
-- ==========================================================================================

; -- Sepparator semicolon before WITH statement

WITH SourceData AS (
    
     -- Identity Module    
    SELECT NEWID() AS ExternalId, 'Identity.Account.ConfirmEmail.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'ConfirmEmail' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.ConfirmEmailChange.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'ConfirmEmailChange' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.ForgotPassword.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'ForgotPassword' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.ForgotPasswordConfirmation.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'ForgotPasswordConfirmation' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.Login.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'Login' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.Logout.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'Logout' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.Register.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'Register' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.RegisterConfirmation.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'RegisterConfirmation' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.ResendEmailConfirmation.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'ResendEmailConfirmation' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.ResetPassword.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'ResetPassword' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.ResetPasswordConfirmation.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'ResetPasswordConfirmation' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.Manage.Index.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'Manage' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.Manage.PersonalData.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'PersonalData' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.Manage.SetPassword.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'SetPassword' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Identity.Account.Manage.Email.Page' AS Name, 'Identity' AS Area, 'Account' AS Controller, 'Email' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
        
    -- Business Incubators Module
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.Index.Page' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'Index' AS Action, 
           NULL AS ParentId, 1 AS IsMenu, 1 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.List.Post' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'List' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.Create.Page' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'Create' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.Edit.Page' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'Edit' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.Delete.Post' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'Delete' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.UpdateDetails.Post' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'UpdateDetails' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.UpdateStatus.Post' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'UpdateStatus' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.UpdateSubscription.Post' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'UpdateSubscription' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL    
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.AddExtraLimit.Post' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'AddExtraLimit' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.DeleteExtraLimit.Post' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'DeleteExtraLimit' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'BusinessIncubators.Home.ClearAllLimits.Post' AS Name, 'BusinessIncubators' AS Area, 'Home' AS Controller, 'ClearAllLimits' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- Projects module
    SELECT NEWID() AS ExternalId, 'Projects.Index.Page' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'Index' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.Index.List.Post' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'List' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.Create.Page+Post' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'Create' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.Edit.Page+Post' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'Edit' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.Delete.Post' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'Delete' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.CopyDiagnosticsForm.Page' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'CopyDiagnosticsForm' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.CopyDiagnosticsForm.Post' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'CopyDiagnosticsFormPost' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.Invitations.Page' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'Invitations' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.ListInvitations.Post' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'ListInvitations' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Projects.ProcessInvitation.Post' AS Name, 'BusinessIncubators' AS Area, 'Projects' AS Controller, 'ProcessInvitation' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    
    -- ProjectStages module
    SELECT NEWID() AS ExternalId, 'ProjectStages.Index.Page' AS Name, 'BusinessIncubators' AS Area, 'ProjectStages' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectStages.Edit.Page+Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectStages' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectStages.Activate.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectStages' AS Controller, 'Activate' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectStages.Deactivate.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectStages' AS Controller, 'Deactivate' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- Diagnostics module
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.LoadCSV.Page+Post' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'LoadCSV' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.List.Page+Post' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'List' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.Create.Page+Post' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'Create' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.Builder.Page' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'Builder' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.AddQuestion.Post' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'AddQuestion' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.RemoveQuestion.Post' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'RemoveQuestion' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.ReorderQuestions.Post' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'ReorderQuestions' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.UpdateQuestion.Post' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'UpdateQuestion' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Forms.GetQuestion.Get' AS Name, 'Diagnostics' AS Area, 'Forms' AS Controller, 'GetQuestion' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    -- Commented out duplicate: Diagnostics.Questions.List already defined below
    -- UNION ALL
    -- SELECT NEWID() AS ExternalId, 'Diagnostics.Questions.List.Page+Post' AS Name, 'Diagnostics' AS Area, 'Questions' AS Controller, 'List' AS Action, 
    --        NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.DiagnosisForms.Index.Page+Post' AS Name, 'Diagnostics' AS Area, 'DiagnosisForms' AS Controller, 'Index' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- Knowledge Structure module
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Modules.Index.Page' AS Name, 'KnowledgeStructure' AS Area, 'Modules' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Modules.List.Post' AS Name, 'KnowledgeStructure' AS Area, 'Modules' AS Controller, 'List' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Modules.Create.Page+Post' AS Name, 'KnowledgeStructure' AS Area, 'Modules' AS Controller, 'Create' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Modules.Edit.Page+Post' AS Name, 'KnowledgeStructure' AS Area, 'Modules' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Modules.Delete.Post' AS Name, 'KnowledgeStructure' AS Area, 'Modules' AS Controller, 'Delete' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Modules.ManageKnowledgeStructures.Page' AS Name, 'KnowledgeStructure' AS Area, 'Modules' AS Controller, 'ManageKnowledgeStructures' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Modules.AddToKnowledgeStructure.Post' AS Name, 'KnowledgeStructure' AS Area, 'Modules' AS Controller, 'AddToKnowledgeStructure' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Modules.RemoveFromKnowledgeStructure.Post' AS Name, 'KnowledgeStructure' AS Area, 'Modules' AS Controller, 'RemoveFromKnowledgeStructure' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- Knowledge Structure Topics
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.Index.Page' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.List.Post' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'List' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.Create.Page+Post' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'Create' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.Edit.Page+Post' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.Delete.Post' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'Delete' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.GetModulesByKnowledgeStructure.Get' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'GetModulesByKnowledgeStructure' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.ManageModules.Page' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'ManageModules' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.AddToModule.Post' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'AddToModule' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Topics.RemoveFromModule.Post' AS Name, 'KnowledgeStructure' AS Area, 'Topics' AS Controller, 'RemoveFromModule' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- Knowledge Structure Subjects
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.Index.Page' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.List.Post' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'List' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.Create.Page+Post' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'Create' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.Edit.Page+Post' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.Delete.Post' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'Delete' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.GetModulesByKnowledgeStructure.Get' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'GetModulesByKnowledgeStructure' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.GetTopicsByModule.Get' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'GetTopicsByModule' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.ManageTopics.Page' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'ManageTopics' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.AddToTopic.Post' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'AddToTopic' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.RemoveFromTopic.Post' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'RemoveFromTopic' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.Resources.Page' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'Resources' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.AddResource.Post' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'AddResource' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.Subjects.RemoveResource.Post' AS Name, 'KnowledgeStructure' AS Area, 'Subjects' AS Controller, 'RemoveResource' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- KnowledgeStructure Management
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.KnowledgeStructure.Index.Page' AS Name, 'KnowledgeStructure' AS Area, 'KnowledgeStructure' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.KnowledgeStructure.List.Post' AS Name, 'KnowledgeStructure' AS Area, 'KnowledgeStructure' AS Controller, 'List' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.KnowledgeStructure.Create.Page+Post' AS Name, 'KnowledgeStructure' AS Area, 'KnowledgeStructure' AS Controller, 'Create' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.KnowledgeStructure.Edit.Page+Post' AS Name, 'KnowledgeStructure' AS Area, 'KnowledgeStructure' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.KnowledgeStructure.Builder.Page' AS Name, 'KnowledgeStructure' AS Area, 'KnowledgeStructure' AS Controller, 'Builder' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.KnowledgeStructure.GetTreeData.Get' AS Name, 'KnowledgeStructure' AS Area, 'KnowledgeStructure' AS Controller, 'GetTreeData' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.KnowledgeStructure.MoveNode.Post' AS Name, 'KnowledgeStructure' AS Area, 'KnowledgeStructure' AS Controller, 'MoveNode' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'KnowledgeStructure.KnowledgeStructure.GetNodeDetails.Get' AS Name, 'KnowledgeStructure' AS Area, 'KnowledgeStructure' AS Controller, 'GetNodeDetails' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- Diagnostics Blocks module
    SELECT NEWID() AS ExternalId, 'Diagnostics.Blocks.Index.Page' AS Name, 'Diagnostics' AS Area, 'Blocks' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Blocks.List.Post' AS Name, 'Diagnostics' AS Area, 'Blocks' AS Controller, 'List' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Blocks.Create.Page+Post' AS Name, 'Diagnostics' AS Area, 'Blocks' AS Controller, 'Create' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Blocks.Edit.Page+Post' AS Name, 'Diagnostics' AS Area, 'Blocks' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Diagnostics.Blocks.Delete.Post' AS Name, 'Diagnostics' AS Area, 'Blocks' AS Controller, 'Delete' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

     -- Default module
    SELECT NEWID() AS ExternalId, 'Default.Home.Error.Page' AS Name, '' AS Area, 'Home' AS Controller, 'Error' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.Home.Index.Page' AS Name, '' AS Area, 'Home' AS Controller, 'Index' AS Action, 
           NULL AS ParentId, 1 AS IsMenu, 1 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.Home.AccessDenied.Page' AS Name, '' AS Area, 'Home' AS Controller, 'AccessDenied' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.Home.PageNotFound.Page' AS Name, '' AS Area, 'Home' AS Controller, 'PageNotFound' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.ContextSelection.Index.Page' AS Name, '' AS Area, 'ContextSelection' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.ContextSelection.GetIncubators.Get' AS Name, '' AS Area, 'ContextSelection' AS Controller, 'GetIncubators' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.ContextSelection.GetProjects.Get' AS Name, '' AS Area, 'ContextSelection' AS Controller, 'GetProjects' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.ContextSelection.LoadContext.Post' AS Name, '' AS Area, 'ContextSelection' AS Controller, 'LoadContext' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.ContextSelection.SelectContext.Post' AS Name, '' AS Area, 'ContextSelection' AS Controller, 'SelectContext' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Default.AuthRedirect.RedirectToDashboard' AS Name, '' AS Area, 'AuthRedirect' AS Controller, 'RedirectToDashboard' AS Action, 
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    
    -- Invitations module (public)
    SELECT NEWID() AS ExternalId, 'Invitations.Accept.Page+Post' AS Name, '' AS Area, 'Invitations' AS Controller, 'Accept' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Invitations.Decline.Post' AS Name, '' AS Area, 'Invitations' AS Controller, 'Decline' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 1 AS IsPublic
    UNION ALL

    -- Project Knowledge Structure module
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.Index.Page' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.Tree.Get' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'Tree' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.BlocksTree.Get' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'BlocksTree' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.MoveNode.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'MoveNode' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.CreateBlock.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'CreateBlock' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.CreateQuestion.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'CreateQuestion' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.CreateModule.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'CreateModule' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.CreateTopic.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'CreateTopic' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.CreateSubject.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'CreateSubject' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.SelectSourceForm.Get' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'SelectSourceForm' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.CopyStructure.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'CopyStructure' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.Clear.Delete' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'Clear' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.UpdateModule.Put' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'UpdateModule' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.UpdateBlock.Put' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'UpdateBlock' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.UpdateQuestion.Put' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'UpdateQuestion' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.UpdateTopic.Put' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'UpdateTopic' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.UpdateSubject.Put' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'UpdateSubject' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.GetAvailableSources.Get' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'GetAvailableSources' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.SyncModule.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'SyncModule' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.GetSourcePreview.Get' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'GetSourcePreview' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.SyncAll.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'SyncAll' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.SyncTopic.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'SyncTopic' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.SyncSubject.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'SyncSubject' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.DeleteBlock.Delete' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'DeleteBlock' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.DeleteQuestion.Delete' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'DeleteQuestion' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.DeleteModule.Delete' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'DeleteModule' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.DeleteTopic.Delete' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'DeleteTopic' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.DeleteSubject.Delete' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'DeleteSubject' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.CreateAnswerOption.Post' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'CreateAnswerOption' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.UpdateAnswerOption.Put' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'UpdateAnswerOption' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.DeleteAnswerOption.Delete' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'DeleteAnswerOption' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ProjectKnowledgeStructure.GetQuestion.Get' AS Name, 'BusinessIncubators' AS Area, 'ProjectKnowledgeStructure' AS Controller, 'GetQuestion' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- Participant Form module
    SELECT NEWID() AS ExternalId, 'ParticipantForm.Index.Page' AS Name, 'BusinessIncubators' AS Area, 'ParticipantForm' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ParticipantForm.SaveDraft.Post' AS Name, 'BusinessIncubators' AS Area, 'ParticipantForm' AS Controller, 'SaveDraft' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ParticipantForm.Submit.Post' AS Name, 'BusinessIncubators' AS Area, 'ParticipantForm' AS Controller, 'Submit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ParticipantForm.GetFormStructure.Get' AS Name, 'BusinessIncubators' AS Area, 'ParticipantForm' AS Controller, 'GetFormStructure' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ParticipantForm.ReplyToFeedback.Post' AS Name, 'BusinessIncubators' AS Area, 'ParticipantForm' AS Controller, 'ReplyToFeedback' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ParticipantForm.CloseFeedback.Post' AS Name, 'BusinessIncubators' AS Area, 'ParticipantForm' AS Controller, 'CloseFeedback' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'ParticipantForm.ReopenFeedback.Post' AS Name, 'BusinessIncubators' AS Area, 'ParticipantForm' AS Controller, 'ReopenFeedback' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL

    -- Form Review module
    SELECT NEWID() AS ExternalId, 'FormReview.Index.Page' AS Name, 'BusinessIncubators' AS Area, 'FormReview' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'FormReview.GetSubmissions.Post' AS Name, 'BusinessIncubators' AS Area, 'FormReview' AS Controller, 'GetSubmissions' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'FormReview.Details.Page' AS Name, 'BusinessIncubators' AS Area, 'FormReview' AS Controller, 'Details' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'FormReview.Approve.Post' AS Name, 'BusinessIncubators' AS Area, 'FormReview' AS Controller, 'Approve' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'FormReview.Reject.Post' AS Name, 'BusinessIncubators' AS Area, 'FormReview' AS Controller, 'Reject' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    
    -- Starter Dashboard module
    SELECT NEWID() AS ExternalId, 'StarterDashboard.Index.Page' AS Name, 'BusinessIncubators' AS Area, 'StarterDashboard' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 1 AS IsMenu, 10 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'StarterDashboard.CompleteTask.Post' AS Name, 'BusinessIncubators' AS Area, 'StarterDashboard' AS Controller, 'CompleteTask' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'StarterDashboard.MarkNotificationRead.Post' AS Name, 'BusinessIncubators' AS Area, 'StarterDashboard' AS Controller, 'MarkNotificationRead' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'StarterDashboard.UpdatePreferences.Post' AS Name, 'BusinessIncubators' AS Area, 'StarterDashboard' AS Controller, 'UpdatePreferences' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'StarterDashboard.GetWidgetData.Get' AS Name, 'BusinessIncubators' AS Area, 'StarterDashboard' AS Controller, 'GetWidgetData' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'StarterDashboard.GetUserProjects.Get' AS Name, 'BusinessIncubators' AS Area, 'StarterDashboard' AS Controller, 'GetUserProjects' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'StarterDashboard.SwitchProject.Post' AS Name, 'BusinessIncubators' AS Area, 'StarterDashboard' AS Controller, 'SwitchProject' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    
    -- Diagnostics Questions module
    SELECT NEWID() AS ExternalId, 'Questions.List.Page' AS Name, 'Diagnostics' AS Area, 'Questions' AS Controller, 'List' AS Action,
           NULL AS ParentId, 1 AS IsMenu, 2 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Questions.Create.Page' AS Name, 'Diagnostics' AS Area, 'Questions' AS Controller, 'Create' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Questions.Edit.Page' AS Name, 'Diagnostics' AS Area, 'Questions' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Questions.Delete.Post' AS Name, 'Diagnostics' AS Area, 'Questions' AS Controller, 'Delete' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    
    -- Coordination Module (Coordinator Journey)
    SELECT NEWID() AS ExternalId, 'Coordination.Default.Index.Get' AS Name, 'Coordination' AS Area, 'Default' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Dashboard.Index.Page' AS Name, 'Coordination' AS Area, 'Dashboard' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 1 AS IsMenu, 1 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Dashboard.GetParticipantStats.Get' AS Name, 'Coordination' AS Area, 'Dashboard' AS Controller, 'GetParticipantStats' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Dashboard.GetDiagnosticStats.Get' AS Name, 'Coordination' AS Area, 'Dashboard' AS Controller, 'GetDiagnosticStats' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Dashboard.GetPendingReviews.Get' AS Name, 'Coordination' AS Area, 'Dashboard' AS Controller, 'GetPendingReviews' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Dashboard.GetRecentActivity.Get' AS Name, 'Coordination' AS Area, 'Dashboard' AS Controller, 'GetRecentActivity' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Dashboard.MarkNotificationRead.Post' AS Name, 'Coordination' AS Area, 'Dashboard' AS Controller, 'MarkNotificationRead' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Dashboard.RefreshWidget.Get' AS Name, 'Coordination' AS Area, 'Dashboard' AS Controller, 'RefreshWidget' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Participant.Index.Page' AS Name, 'Coordination' AS Area, 'Participant' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 1 AS IsMenu, 2 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Participant.List.Post' AS Name, 'Coordination' AS Area, 'Participant' AS Controller, 'List' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Participant.BulkInvite.Page+Post' AS Name, 'Coordination' AS Area, 'Participant' AS Controller, 'BulkInvite' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Participant.DownloadTemplate.Get' AS Name, 'Coordination' AS Area, 'Participant' AS Controller, 'DownloadTemplate' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Participant.Export.Get' AS Name, 'Coordination' AS Area, 'Participant' AS Controller, 'Export' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Participant.GetStats.Get' AS Name, 'Coordination' AS Area, 'Participant' AS Controller, 'GetStats' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Reports.Index.Page' AS Name, 'Coordination' AS Area, 'Reports' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 1 AS IsMenu, 3 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Reports.GetTemplates.Get' AS Name, 'Coordination' AS Area, 'Reports' AS Controller, 'GetTemplates' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Reports.Generate.Post' AS Name, 'Coordination' AS Area, 'Reports' AS Controller, 'Generate' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Reports.Export.Post' AS Name, 'Coordination' AS Area, 'Reports' AS Controller, 'Export' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Reports.CreateTemplate.Post' AS Name, 'Coordination' AS Area, 'Reports' AS Controller, 'CreateTemplate' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Reports.GetStats.Get' AS Name, 'Coordination' AS Area, 'Reports' AS Controller, 'GetStats' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.Index.Page' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 1 AS IsMenu, 4 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.GetPendingReviews.Post' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'GetPendingReviews' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.Review.Get' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'Review' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.GetSubmissionDetails.Get' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'GetSubmissionDetails' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.AddFeedback.Post' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'AddFeedback' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.Approve.Post' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'Approve' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.RequestChanges.Post' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'RequestChanges' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.ReplyToFeedback.Post' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'ReplyToFeedback' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.CloseFeedback.Post' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'CloseFeedback' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.FormReview.ReopenFeedback.Post' AS Name, 'Coordination' AS Area, 'FormReview' AS Controller, 'ReopenFeedback' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    
    -- User Management Module (Coordination Area)
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.Index.Page' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 1 AS IsMenu, 5 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.Details.Page' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'Details' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.Create.Page' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'Create' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.Edit.Page' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.Deactivate.Post' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'Deactivate' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.Reactivate.Post' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'Reactivate' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.UpdateAvatar.Post' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'UpdateAvatar' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.ListUsers.Post' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'ListUsers' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.BulkImport.Page+Post' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'BulkImport' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.BulkImportProgress.Get' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'BulkImportProgress' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.ExportUsers.Get' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'ExportUsers' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.ManageRoles.Page+Post' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'ManageRoles' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.BatchRoleAssignment.Post' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'BatchRoleAssignment' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.GetAllIncubators.Get' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'GetAllIncubators' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.UserManagement.GetProjectsByIncubator.Get' AS Name, 'Coordination' AS Area, 'UserManagement' AS Controller, 'GetProjectsByIncubator' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    
    -- Email Template Module (Coordination Area)
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.EmailTemplate.Index.Page' AS Name, 'Coordination' AS Area, 'EmailTemplate' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.EmailTemplate.Details.Page' AS Name, 'Coordination' AS Area, 'EmailTemplate' AS Controller, 'Details' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.EmailTemplate.Create.Page+Post' AS Name, 'Coordination' AS Area, 'EmailTemplate' AS Controller, 'Create' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.EmailTemplate.Edit.Page+Post' AS Name, 'Coordination' AS Area, 'EmailTemplate' AS Controller, 'Edit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.EmailTemplate.Delete.Post' AS Name, 'Coordination' AS Area, 'EmailTemplate' AS Controller, 'Delete' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.EmailTemplate.Preview.Get' AS Name, 'Coordination' AS Area, 'EmailTemplate' AS Controller, 'Preview' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    
    -- Audit Module (Coordination Area)
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Audit.Index.Page' AS Name, 'Coordination' AS Area, 'Audit' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Audit.EntityHistory.Page' AS Name, 'Coordination' AS Area, 'Audit' AS Controller, 'EntityHistory' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Audit.UserActivity.Page' AS Name, 'Coordination' AS Area, 'Audit' AS Controller, 'UserActivity' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Coordination.Audit.Export.Get' AS Name, 'Coordination' AS Area, 'Audit' AS Controller, 'Export' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    
    -- Participant Module (For Starters)
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Participant.Dashboard.Index.Page' AS Name, 'Participant' AS Area, 'Dashboard' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Participant.Dashboard.MyProjects.Get' AS Name, 'Participant' AS Area, 'Dashboard' AS Controller, 'MyProjects' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Participant.Dashboard.PendingForms.Get' AS Name, 'Participant' AS Area, 'Dashboard' AS Controller, 'PendingForms' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'Participant.Dashboard.OpenConvocations.Get' AS Name, 'Participant' AS Area, 'Dashboard' AS Controller, 'OpenConvocations' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    
    -- Entrepreneur Form Module (Redesigned with ExternalId)
    UNION ALL
    SELECT NEWID() AS ExternalId, 'EntrepreneurForm.Start.Get' AS Name, NULL AS Area, 'EntrepreneurForm' AS Controller, 'Start' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'EntrepreneurForm.Index.Page' AS Name, NULL AS Area, 'EntrepreneurForm' AS Controller, 'Index' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'EntrepreneurForm.SaveDraft.Post' AS Name, NULL AS Area, 'EntrepreneurForm' AS Controller, 'SaveDraft' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'EntrepreneurForm.Submit.Post' AS Name, NULL AS Area, 'EntrepreneurForm' AS Controller, 'Submit' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'EntrepreneurForm.GetProgress.Get' AS Name, NULL AS Area, 'EntrepreneurForm' AS Controller, 'GetProgress' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'EntrepreneurForm.Success.Get' AS Name, NULL AS Area, 'EntrepreneurForm' AS Controller, 'Success' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
    UNION ALL
    SELECT NEWID() AS ExternalId, 'EntrepreneurForm.View.Get' AS Name, NULL AS Area, 'EntrepreneurForm' AS Controller, 'View' AS Action,
           NULL AS ParentId, 0 AS IsMenu, 0 AS MenuOrder, 0 AS IsPublic
)
MERGE INTO [systemfeatures].WebFeatures AS target
USING SourceData AS source
ON target.Area = source.Area AND target.Controller = source.Controller AND target.Action = source.Action

WHEN MATCHED THEN 
    UPDATE SET 
        target.Name = source.Name,
        target.ParentId = source.ParentId,
        target.IsMenu = source.IsMenu,
        target.MenuOrder = source.MenuOrder,
        target.IsPublic = source.IsPublic

WHEN NOT MATCHED THEN 
    INSERT (ExternalId, Name, Area, Controller, Action, ParentId, IsMenu, MenuOrder, IsPublic)
    VALUES (source.ExternalId, source.Name, source.Area, source.Controller, source.Action, source.ParentId, source.IsMenu, source.MenuOrder, source.IsPublic);


-- Ensure all non-public WebFeatures have a corresponding ProtectedResources record
MERGE INTO [permissions].ProtectedResources AS target
USING [systemfeatures].WebFeatures AS source
ON target.ExternalId = source.ExternalId AND source.IsPublic = 0

WHEN MATCHED THEN 
    UPDATE SET target.Name = source.Name

WHEN NOT MATCHED AND source.IsPublic = 0 THEN 
    INSERT (ExternalId, Name, ResourceType)
    VALUES (source.ExternalId, source.Name, 1); -- 1: WebFeature
