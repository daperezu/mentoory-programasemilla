// ReSharper disable InconsistentNaming
namespace LinaSys.Shared.Application;

/// <summary>
/// Enumeration of error codes for result operations.
/// </summary>
public enum ResultErrorCodes
{
    /// <summary>
    /// Indicates an unknown error.
    /// </summary>
    Unknown = 000_000,

    /// <summary>
    /// Indicates a generic error.
    /// </summary>
    GenericError = 000_001,

    /// <summary>
    /// Indicates a validation error.
    /// </summary>
    Validation_SomeFieldsAreInvalid = 100_010,

    /// <summary>
    /// Indicates that a Business Incubator with the specified name already exists.
    /// </summary>
    BusinessIncubator_NameAlreadyExists = 200_010,

    /// <summary>
    /// Indicates that a Business Incubator with the specified key already exists.
    /// </summary>
    BusinessIncubator_KeyAlreadyExists = 200_020,

    /// <summary>
    /// Indicates that a Business Incubator was not found.
    /// </summary>
    BusinessIncubator_NotFound = 200_030,

    /// <summary>
    /// Indicates that a Business Incubator restore failed.
    /// </summary>
    BusinessIncubator_RestoreFailed = 200_040,

    /// <summary>
    /// Indicates that a Business Incubator delete failed.
    /// </summary>
    BusinessIncubator_DeleteFailed = 200_050,

    /// <summary>
    /// Indicates that a query for Business Incubator failed.
    /// </summary>
    BusinessIncubator_FilterQueryFailed = 200_060,

    /// <summary>
    /// Indicates that a project creation failed.
    /// </summary>
    Project_CreationFailed = 300_010,

    /// <summary>
    /// Indicates that a project update failed.
    /// </summary>
    Project_UpdateFailed = 300_020,

    /// <summary>
    /// Indicates that a project deletion failed.
    /// </summary>
    Project_DeleteFailed = 300_030,

    /// <summary>
    /// Indicates that a project restoration failed.
    /// </summary>
    Project_RestoreFailed = 300_040,

    /// <summary>
    /// Indicates that a project was not found.
    /// </summary>
    Project_NotFound = 300_050,

    /// <summary>
    /// Indicates that the project has already a knowledge structure assigned.
    /// </summary>
    Project_AlreadyAssigned = 300_055,

    /// <summary>
    /// Indicates that a project status change failed.
    /// </summary>
    Project_ChangeStatusFailed = 300_060,

    /// <summary>
    /// Indicates that project processing failed.
    /// </summary>
    Project_ProcessingFailed = 300_070,

    /// <summary>
    /// Indicates that a duplicate invitation already exists.
    /// </summary>
    Project_DuplicateInvitation = 300_080,

    /// <summary>
    /// Indicates that the project's knowledge structure was not found.
    /// </summary>
    Project_KnowledgeStructureNotFound = 300_090,

    /// <summary>
    /// Indicates that a project form submission was not found.
    /// </summary>
    ProjectFormSubmission_NotFound = 301_010,

    /// <summary>
    /// Indicates that the project form submission has an invalid status.
    /// </summary>
    ProjectFormSubmission_InvalidStatus = 301_020,

    /// <summary>
    /// Indicates that the project form submission save failed.
    /// </summary>
    ProjectFormSubmission_SaveFailed = 301_030,

    /// <summary>
    /// Indicates that the requested phase is invalid for the current stage.
    /// </summary>
    ProjectFormSubmission_InvalidPhase = 301_040,

    /// <summary>
    /// Indicates that the form submission is outside the allowed submission window.
    /// </summary>
    ProjectFormSubmission_OutsideWindow = 301_050,

    /// <summary>
    /// Indicates that the form submission has already been submitted.
    /// </summary>
    ProjectFormSubmission_AlreadySubmitted = 301_060,

    /// <summary>
    /// Indicates that the form submission has no draft data.
    /// </summary>
    ProjectFormSubmission_NoDraftData = 301_070,

    /// <summary>
    /// Indicates that the form submission is incomplete.
    /// </summary>
    ProjectFormSubmission_Incomplete = 301_080,

    /// <summary>
    /// Indicates that the form submission cannot be edited.
    /// </summary>
    ProjectFormSubmission_CannotEdit = 301_090,

    /// <summary>
    /// Indicates that the user is not a participant of the project.
    /// </summary>
    ProjectFormSubmission_NotParticipant = 301_100,

    /// <summary>
    /// Indicates that the project has no active stage.
    /// </summary>
    ProjectFormSubmission_NoActiveStage = 301_110,

    /// <summary>
    /// Indicates that the stage is not active.
    /// </summary>
    ProjectFormSubmission_StageNotActive = 301_120,

    /// <summary>
    /// Indicates that the submission window has not started yet.
    /// </summary>
    ProjectFormSubmission_BeforeWindow = 301_130,

    /// <summary>
    /// Indicates that the submission window has ended.
    /// </summary>
    ProjectFormSubmission_AfterWindow = 301_140,

    /// <summary>
    /// Indicates that the form has already been approved.
    /// </summary>
    ProjectFormSubmission_AlreadyApproved = 301_150,

    /// <summary>
    /// Indicates that a user registration failed.
    /// </summary>
    Auth_UserRegistrationFailed = 400_010,

    /// <summary>
    /// Indicates that a protected resource was not found.
    /// </summary>
    Auth_ProtectedResourceNotFound = 400_020,

    /// <summary>
    /// Indicates that the roles have no access to the protected resource.
    /// </summary>
    Auth_RolesHasNoAccessToProtectedResource = 400_030,

    /// <summary>
    /// Indicates that the user has no access to the protected resource.
    /// </summary>
    Auth_UserHasNoAccessToProtectedResource = 400_040,

    /// <summary>
    /// Indicates that the user was not found.
    /// </summary>
    Auth_UserNotFound = 400_050,

    /// <summary>
    /// Indicates that a user profile creation failed.
    /// </summary>
    Auth_UserProfileCreationFailed = 400_060,

    /// <summary>
    /// Indicates that a user query operation failed.
    /// </summary>
    Auth_QueryFailed = 400_070,

    /// <summary>
    /// Indicates that token generation failed.
    /// </summary>
    Auth_TokenGenerationFailed = 400_080,

    /// <summary>
    ///  Indicates that a subscription package creation failed.
    /// </summary>
    Subscription_PackageCreationFailed = 500_010,

    /// <summary>
    /// Indicates that a subscription package update failed.
    /// </summary>
    Subscription_PackageNotFound = 500_020,

    /// <summary>
    /// Indicates that a subscription package update failed.
    /// </summary>
    Subscription_PackageUpdateFailed = 500_030,

    /// <summary>
    /// Indicates that a subscription package was not found.
    /// </summary>
    Subscription_PackageVersionNotFound = 500_040,

    /// <summary>
    /// Indicates that a pack version upsert failed.
    /// </summary>
    Subscription_PackageVersionUpsertFailed = 500_050,

    /// <summary>
    /// Indicates that a Business Incubator Package creation failed.
    /// </summary>
    Subscription_BusinessIncubatorPackageCreateFailed = 500_060,

    /// <summary>
    /// Indicates that a Business Incubator already has a package.
    /// </summary>
    Subscription_BusinessIncubatorAlreadyHasPackage = 500_070,

    /// <summary>
    /// Indicates that a Business Incubator Package was not found.
    /// </summary>
    Subscription_BusinessIncubatorPackageNotFound = 500_080,

    /// <summary>
    /// Indicates that a Business Incubator Package add limit failed.
    /// </summary>
    Subscription_BusinessIncubatorAddLimitFailed = 500_090,

    /// <summary>
    /// Indicates that a Business Incubator Package remove limit failed.
    /// </summary>
    Subscription_BusinessIncubatorRemoveLimitFailed = 500_100,

    /// <summary>
    /// Indicates that a Business Incubator Package clear limits failed.
    /// </summary>
    Subscription_BusinessIncubatorClearLimitsFailed = 500_110,

    /// <summary>
    /// Indicates that a Business Incubator Package switch version failed.
    /// </summary>
    Subscription_BusinessIncubatorSwitchVersionFailed = 500_120,

    /// <summary>
    /// Indicates that a Business Incubator Package get effective limit failed.
    /// </summary>
    Subscription_GetEffectiveLimitFailed = 500_130,

    /// <summary>
    /// Indicate that a Module with the same name already exists.
    /// </summary>
    Module_NameAlreadyExists = 600_010,

    /// <summary>
    /// Indicates that the Module was not found.
    /// </summary>
    Module_NotFound = 600_020,

    /// <summary>
    /// Indicates that a module already exists in the knowledge structure.
    /// </summary>
    Module_AlreadyExists = 600_030,

    /// <summary>
    /// Indicates that a module has dependencies and cannot be removed.
    /// </summary>
    Module_HasDependencies = 600_040,

    /// <summary>
    /// Indicates a block with the same name already exists.
    /// </summary>
    Block_NameAlreadyExists = 700_010,

    /// <summary>
    /// Indicates that the block was not found.
    /// </summary>
    Block_NotFound = 700_020,

    /// <summary>
    /// Indicates that a block cannot be deleted because it is being used.
    /// </summary>
    Block_CannotDeleteUsedBlock = 700_030,

    /// <summary>
    /// Indicates that the block was not found.
    /// </summary>
    DiagnosisForm_Blocks_NotFound = 700_040,

    /// <summary>
    /// Indicates that a diagnosis form with the same name already exists.
    /// </summary>
    DiagnosisForm_NameAlreadyExists = 800_010,

    /// <summary>
    /// Indicates that the diagnosis form was not found.
    /// </summary>
    DiagnosisForm_NotFound = 800_020,

    /// <summary>
    /// Indicates that the question was not found.
    /// </summary>
    Question_NotFound = 800_030,

    /// <summary>
    /// Indicates that the answer was not found.
    /// </summary>
    AnswerOption_NotFound = 800_040,

    /// <summary>
    /// Indicates that the diagnosis form CSV file is invalid.
    /// </summary>
    DiagnosisForm_InvalidCsv = 800_050,

    /// <summary>
    /// Indicates that the question was not found in the form.
    /// </summary>
    DiagnosisForm_QuestionNotFound = 800_060,

    /// <summary>
    /// Indicates that the knowledge structure was not found.
    /// </summary>
    KnowledgeStructure_NotFound = 900_010,

    /// <summary>
    /// Indicates that a knowledge structure with the same name already exists.
    /// </summary>
    KnowledgeStructure_NameAlreadyExists = 900_011,

    /// <summary>
    /// Indicates that a topic with the same name already exists in the module.
    /// </summary>
    Topic_NameAlreadyExists = 900_020,

    /// <summary>
    /// Indicates that the topic was not found.
    /// </summary>
    Topic_NotFound = 900_030,

    /// <summary>
    /// Indicates that the topic cannot be deleted because it has subjects.
    /// </summary>
    Topic_CannotDeleteWithSubjects = 900_040,

    /// <summary>
    /// Indicates that the topic already exists in the module.
    /// </summary>
    Topic_AlreadyExists = 900_050,

    /// <summary>
    /// Indicates that the topic has dependencies and cannot be removed.
    /// </summary>
    Topic_HasDependencies = 900_060,

    /// <summary>
    /// Indicates that a Subject with the same title already exists.
    /// </summary>
    Subject_TitleAlreadyExists = 910_010,

    /// <summary>
    /// Indicates that a Subject was not found.
    /// </summary>
    Subject_NotFound = 910_020,

    /// <summary>
    /// Indicates that a Subject cannot be deleted because it has resources.
    /// </summary>
    Subject_CannotDeleteWithResources = 910_030,

    /// <summary>
    /// Indicates that a Subject already exists in the topic.
    /// </summary>
    Subject_AlreadyExists = 910_035,

    /// <summary>
    /// Indicates that a SubjectResource was not found.
    /// </summary>
    SubjectResource_NotFound = 910_040,

    /// <summary>
    /// Indicates that a protected resource with the same ExternalId already exists.
    /// </summary>
    ProtectedResource_AlreadyExists = 950_010,

    /// <summary>
    /// Indicates that a protected resource was not found.
    /// </summary>
    ProtectedResource_NotFound = 950_020,

    /// <summary>
    /// Indicates that a permission grant operation failed.
    /// </summary>
    Permission_GrantFailed = 950_030,

    /// <summary>
    /// Indicates that a permission revoke operation failed.
    /// </summary>
    Permission_RevokeFailed = 950_040,

    /// <summary>
    /// Indicates a validation error with details in message.
    /// </summary>
    ValidationError = 100_001,

    /// <summary>
    /// Indicates that the entity is already inactive.
    /// </summary>
    AlreadyInactive = 100_002,

    /// <summary>
    /// Indicates that the entity is already active.
    /// </summary>
    AlreadyActive = 100_003,

    /// <summary>
    /// Indicates that the user was not found.
    /// </summary>
    User_NotFound = 110_001,

    /// <summary>
    /// Indicates that the role was not found in the system.
    /// </summary>
    Role_NotFound = 110_010,

    /// <summary>
    /// Indicates that role assignment failed.
    /// </summary>
    Role_AssignmentFailed = 110_020,

    /// <summary>
    /// Indicates that role removal failed.
    /// </summary>
    Role_RemovalFailed = 110_030,

    /// <summary>
    /// Indicates that the user email already exists.
    /// </summary>
    User_EmailAlreadyExists = 110_040,

    /// <summary>
    /// Indicates that the email change failed.
    /// </summary>
    User_EmailChangeFailed = 110_050,

    /// <summary>
    /// Indicates that the new email is the same as current.
    /// </summary>
    User_EmailNotChanged = 110_060,

    /// <summary>
    /// Indicates that the identification is invalid format.
    /// </summary>
    User_InvalidIdentification = 110_070,

    /// <summary>
    /// Indicates that the identification already exists.
    /// </summary>
    User_IdentificationAlreadyExists = 110_080,

    /// <summary>
    /// Indicates that the identification change failed.
    /// </summary>
    User_IdentificationChangeFailed = 110_090,

    /// <summary>
    /// Indicates that the new identification is the same as current.
    /// </summary>
    User_IdentificationNotChanged = 110_100,

    /// <summary>
    /// Indicates that sending a notification failed.
    /// </summary>
    Notification_SendFailed = 120_010,
}
