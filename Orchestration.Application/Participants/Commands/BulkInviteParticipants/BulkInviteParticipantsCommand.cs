using LinaSys.Auth.Application.Commands;
using LinaSys.Auth.Application.Queries;
using LinaSys.BusinessIncubator.Application.Project.Commands;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Notification.Application.Commands;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace LinaSys.Orchestration.Application.Participants.Commands.BulkInviteParticipants;

/// <summary>
/// Command to process bulk participant invitations from file upload.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record BulkInviteParticipantsCommand(
    long ProjectId,
    Stream FileStream,
    string FileName,
    bool SendInvitationEmails,
    string CoordinatorUserId) : IBaseRequest<BulkInviteResultDto>;

/// <summary>
/// DTO for bulk invitation results.
/// </summary>
public class BulkInviteResultDto
{
    /// <summary>
    /// Gets or sets the batch registration ID.
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// Gets or sets the total number of invitations processed.
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of successful invitations.
    /// </summary>
    public int SuccessfulInvitations { get; set; }

    /// <summary>
    /// Gets or sets the number of failed invitations.
    /// </summary>
    public int FailedInvitations { get; set; }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// Gets or sets the processing errors.
    /// </summary>
    public List<string> ProcessingErrors { get; set; } = [];

    /// <summary>
    /// Gets or sets the detailed results for each user.
    /// </summary>
    public List<UserInvitationResult> DetailedResults { get; set; } = [];
}

/// <summary>
/// Result for each user invitation.
/// </summary>
public class UserInvitationResult
{
    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the user already existed.
    /// </summary>
    public bool UserAlreadyExisted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether an invitation was created.
    /// </summary>
    public bool InvitationCreated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether an email was sent.
    /// </summary>
    public bool EmailSent { get; set; }

    /// <summary>
    /// Gets or sets the error message if any.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the invitation token.
    /// </summary>
    public string? InvitationToken { get; set; }
}

/// <summary>
/// Invitation data from CSV/Excel file.
/// </summary>
public class InvitationData
{
    /// <summary>
    /// Gets or sets the participant email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant identification number.
    /// </summary>
    public string IdentificationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the row number for error reporting.
    /// </summary>
    public int RowNumber { get; set; }
}

/// <summary>
/// Handler for BulkInviteParticipantsCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BulkInviteParticipantsCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
/// <param name="mediator">The mediator instance.</param>
/// <param name="emailTemplateService">The email template service.</param>
/// <param name="passwordGeneratorService">The password generator service.</param>
/// <param name="auditContext">The audit context.</param>
/// <param name="urlService">The application URL service.</param>
/// <param name="timeProvider">The time provider.</param>
/// <param name="logger">The logger instance.</param>
public class BulkInviteParticipantsCommandHandler(
    IBusinessIncubatorRepository repository,
    IMediator mediator,
    IEmailTemplateService emailTemplateService,
    IPasswordGeneratorService passwordGeneratorService,
    IAuditContext auditContext,
    IApplicationUrlService urlService,
    ITimeProvider timeProvider,
    ILogger<BulkInviteParticipantsCommandHandler> logger) : BaseCommandHandler<BulkInviteParticipantsCommand, BulkInviteResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<BulkInviteResultDto>> Handle(
        BulkInviteParticipantsCommand request,
        CancellationToken cancellationToken)
    {
        // Validate project exists
        var project = await repository.GetProjectWithUsersAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                (nameof(BulkInviteParticipantsCommand), $"Project with ID {request.ProjectId} not found."));
        }

        var result = new BulkInviteResultDto
        {
            BatchId = Guid.NewGuid()
        };

        try
        {
            // Parse CSV/Excel file
            var invitationData = await ParseInvitationFile(request.FileStream, request.FileName);
            result.TotalProcessed = invitationData.Count;

            // Validate data
            var validationErrors = ValidateInvitationData(invitationData);
            result.ValidationErrors = validationErrors;

            if (validationErrors.Any())
            {
                result.FailedInvitations = result.TotalProcessed;
                return Success(result);
            }

            // Create batch registration record
            var batchRegistration = project.CreateBatchUserRegistration(
                request.FileName,
                invitationData.Count,
                auditContext);

            batchRegistration.StartProcessing(auditContext.UtcNow);

            // Save the batch registration through the project (which is already tracked)
            await repository.UpdateAsync(project, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
            result.BatchId = batchRegistration.ExternalId;

            // Process users with progress tracking (always use Starter role)
            var processedResults = await ProcessUsersAsync(
                invitationData,
                project,
                batchRegistration,
                request.SendInvitationEmails,
                request.CoordinatorUserId,
                cancellationToken);

            // Final update
            var successCount = processedResults.Count(r => r.Success);
            var failedCount = processedResults.Count(r => !r.Success);

            batchRegistration.UpdateProgress(
                processedResults.Count,
                successCount,
                failedCount);

            batchRegistration.Complete(auditContext.UtcNow);

            // Save final state
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            result.SuccessfulInvitations = successCount;
            result.FailedInvitations = failedCount;
            result.ProcessingErrors = processedResults
                .Where(r => !r.Success && !string.IsNullOrEmpty(r.ErrorMessage))
                .Select(r => $"{r.Email}: {r.ErrorMessage}")
                .Take(10) // Limit errors shown
                .ToList();
            result.DetailedResults = processedResults;

            return Success(result);
        }
        catch (Exception ex)
        {
            result.ProcessingErrors.Add($"Error processing file: {ex.Message}");
            result.FailedInvitations = result.TotalProcessed;
            return Success(result);
        }
    }

    /// <summary>
    /// Parses the invitation file (CSV or Excel).
    /// </summary>
    /// <param name="fileStream">The file stream.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>List of invitation data.</returns>
    private static async Task<List<InvitationData>> ParseInvitationFile(Stream fileStream, string fileName)
    {
        var invitations = new List<InvitationData>();
        var isExcel = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

        if (isExcel)
        {
            // Parse Excel file using EPPlus
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                return invitations;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;

            // Start from row 2 to skip header
            for (var row = 2; row <= rowCount; row++)
            {
                var email = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? string.Empty;
                var firstName = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? string.Empty;
                var lastName = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? string.Empty;
                var identificationNumber = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? string.Empty;

                // Skip empty rows
                if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
                {
                    continue;
                }

                invitations.Add(new InvitationData
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    IdentificationNumber = identificationNumber,
                    RowNumber = row
                });
            }
        }
        else
        {
            // Parse CSV file
            using var reader = new StreamReader(fileStream);
            var rowNumber = 0;
            var isFirstRow = true;

            while (await reader.ReadLineAsync() is { } line)
            {
                rowNumber++;

                if (isFirstRow)
                {
                    isFirstRow = false;
                    continue; // Skip header row
                }

                var columns = line.Split(',');
                if (columns.Length >= 4)
                {
                    invitations.Add(new InvitationData
                    {
                        Email = columns[0].Trim().Trim('"'),
                        FirstName = columns[1].Trim().Trim('"'),
                        LastName = columns[2].Trim().Trim('"'),
                        IdentificationNumber = columns[3].Trim().Trim('"'),
                        RowNumber = rowNumber
                    });
                }
            }
        }

        return invitations;
    }

    /// <summary>
    /// Validates the invitation data.
    /// </summary>
    /// <param name="invitations">The invitation data.</param>
    /// <returns>List of validation errors.</returns>
    private static List<string> ValidateInvitationData(List<InvitationData> invitations)
    {
        var errors = new List<string>();

        foreach (var invitation in invitations)
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(invitation.Email))
            {
                errors.Add($"Fila {invitation.RowNumber}: El email es requerido.");
            }
            else if (!IsValidEmail(invitation.Email))
            {
                errors.Add($"Fila {invitation.RowNumber}: El email '{invitation.Email}' no es válido.");
            }

            // Validate names
            if (string.IsNullOrWhiteSpace(invitation.FirstName))
            {
                errors.Add($"Fila {invitation.RowNumber}: El nombre es requerido.");
            }

            if (string.IsNullOrWhiteSpace(invitation.LastName))
            {
                errors.Add($"Fila {invitation.RowNumber}: El apellido es requerido.");
            }

            // Validate identification number
            if (string.IsNullOrWhiteSpace(invitation.IdentificationNumber))
            {
                errors.Add($"Fila {invitation.RowNumber}: El número de identificación es requerido.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates if the email format is correct.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Processes users for batch registration with progress tracking.
    /// </summary>
    private async Task<List<UserInvitationResult>> ProcessUsersAsync(
        List<InvitationData> invitations,
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project project,
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.BatchUserRegistration batchRegistration,
        bool sendEmails,
        string coordinatorUserId,
        CancellationToken cancellationToken)
    {
        var results = new List<UserInvitationResult>();
        var processedCount = 0;
        var successCount = 0;
        var failedCount = 0;
        const int saveProgressInterval = 10; // Save progress every 10 users

        // Get all emails to check which users already exist
        var emails = invitations.Select(i => i.Email).ToList();
        var getUsersQuery = new GetUsersByEmailsQuery(emails);
        var usersResult = await mediator.Send(getUsersQuery, cancellationToken);

        if (!usersResult.IsSuccess)
        {
            logger.LogError("Failed to query existing users");
            foreach (var invitation in invitations)
            {
                results.Add(new UserInvitationResult
                {
                    Email = invitation.Email,
                    FullName = $"{invitation.FirstName} {invitation.LastName}",
                    Success = false,
                    ErrorMessage = "Error al verificar usuarios existentes"
                });
            }

            return results;
        }

        var existingUsers = usersResult.Value!.ExistingUsers;
        var nonExistentEmails = usersResult.Value!.NonExistentEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Process each invitation
        foreach (var invitation in invitations)
        {
            try
            {
                // Always use Starter role
                var role = Roles.Starter;
                var fullName = $"{invitation.FirstName} {invitation.LastName}";
                var existingUser = existingUsers.Values.FirstOrDefault(u =>
                    u.Email!.Equals(invitation.Email, StringComparison.OrdinalIgnoreCase));

                var userAlreadyExisted = existingUser != null;
                string? temporaryPassword = null;
                var userId = existingUser?.Id;

                // Step 1: Create user if they don't exist
                if (!userAlreadyExisted && nonExistentEmails.Contains(invitation.Email))
                {
                    temporaryPassword = passwordGeneratorService.GeneratePassword();
                    var createUserCommand = new CreateUserCommand(
                        Email: invitation.Email,
                        Password: temporaryPassword,
                        Username: invitation.IdentificationNumber,
                        EmailConfirmed: false);

                    var createResult = await mediator.Send(createUserCommand, cancellationToken);

                    if (!createResult.IsSuccess)
                    {
                        var errorMessage = string.Join("; ", createResult.ErrorMessages?.Select(em => em.Message) ?? ["Error creando usuario"]);
                        logger.LogError("Failed to create user {Email}: {Error}", invitation.Email, errorMessage);

                        results.Add(new UserInvitationResult
                        {
                            Email = invitation.Email,
                            FullName = fullName,
                            Success = false,
                            UserAlreadyExisted = false,
                            InvitationCreated = false,
                            EmailSent = false,
                            ErrorMessage = errorMessage
                        });

                        failedCount++;
                        processedCount++;
                        continue;
                    }

                    userId = createResult.Value!.Id;
                    // Update existingUsers for future lookups
                    existingUsers[invitation.Email] = createResult.Value!;
                }

                // Step 2: Create project invitation using the proper command
                var createInvitationCommand = new CreateProjectInvitationCommand(
                    project.ExternalId,
                    invitation.Email,
                    fullName,
                    invitation.IdentificationNumber,
                    role,
                    7); // 7 days expiration

                var invitationResult = await mediator.Send(createInvitationCommand, cancellationToken);

                if (!invitationResult.IsSuccess)
                {
                    var errorMessage = string.Join("; ", invitationResult.ErrorMessages?.Select(em => em.Message) ?? ["Error creando invitación"]);
                    logger.LogError("Failed to create invitation for {Email}: {Error}", invitation.Email, errorMessage);

                    results.Add(new UserInvitationResult
                    {
                        Email = invitation.Email,
                        FullName = fullName,
                        Success = false,
                        UserAlreadyExisted = userAlreadyExisted,
                        InvitationCreated = false,
                        EmailSent = false,
                        ErrorMessage = errorMessage
                    });

                    failedCount++;
                    processedCount++;
                    continue;
                }

                var invitationToken = invitationResult.Value!;

                // Step 3: Add user to project and publish integration event
                if (userId != null)
                {
                    var addUserResult = await AddExistingUserToProjectAsync(
                        project,
                        userId,
                        invitation.Email,
                        fullName,
                        role,
                        coordinatorUserId,
                        cancellationToken);

                    if (!addUserResult)
                    {
                        logger.LogWarning("User {Email} created but could not be added to project", invitation.Email);
                    }
                }

                // Step 4: Send email if requested
                var emailSent = false;
                if (sendEmails)
                {
                    try
                    {
                        var invitationLink = urlService.GetInvitationAcceptanceUrl(invitationToken);
                        string emailContent;
                        string emailSubject;

                        if (userAlreadyExisted)
                        {
                            // Send project invitation email
                            emailContent = emailTemplateService.GenerateProjectInvitationEmail(
                                fullName,
                                project.Name,
                                invitationLink);
                            emailSubject = $"Invitación al Proyecto {project.Name}";
                        }
                        else
                        {
                            // Send account creation with project invitation email
                            emailContent = emailTemplateService.GenerateAccountCreationEmail(
                                fullName,
                                invitation.Email,
                                temporaryPassword ?? string.Empty,
                                invitationLink,
                                project.Name);
                            emailSubject = $"Bienvenido al Proyecto {project.Name}";
                        }

                        var emailCommand = new SendEmailCommand(
                            To: invitation.Email,
                            Subject: emailSubject,
                            Body: emailContent);

                        var emailResult = await mediator.Send(emailCommand, cancellationToken);
                        emailSent = emailResult.IsSuccess;

                        if (!emailSent)
                        {
                            logger.LogWarning("Failed to send email to {Email}", invitation.Email);
                        }
                    }
                    catch (Exception emailEx)
                    {
                        logger.LogError(emailEx, "Error sending email to {Email}", invitation.Email);
                    }
                }

                results.Add(new UserInvitationResult
                {
                    Email = invitation.Email,
                    FullName = fullName,
                    Success = true,
                    UserAlreadyExisted = userAlreadyExisted,
                    InvitationCreated = true,
                    EmailSent = emailSent,
                    InvitationToken = invitationToken
                });

                successCount++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing invitation for {Email}", invitation.Email);
                results.Add(new UserInvitationResult
                {
                    Email = invitation.Email,
                    FullName = $"{invitation.FirstName} {invitation.LastName}",
                    Success = false,
                    ErrorMessage = "Error interno al procesar invitación"
                });

                failedCount++;
            }

            processedCount++;

            // Save progress periodically
            if (processedCount % saveProgressInterval == 0)
            {
                try
                {
                    batchRegistration.UpdateProgress(processedCount, successCount, failedCount);

                    // Save progress through the already-tracked project
                    await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                    logger.LogInformation("Saved batch registration progress: {Processed}/{Total} users", processedCount, invitations.Count);
                }
                catch (Exception saveEx)
                {
                    logger.LogError(saveEx, "Failed to save batch registration progress");
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Adds an existing user to the project.
    /// </summary>
    private async Task<bool> AddExistingUserToProjectAsync(
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project project,
        string userId,
        string userEmail,
        string userName,
        string role,
        string invitedBy,
        CancellationToken cancellationToken)
    {
        try
        {
            // Reload project with users if needed
            if (!project.ProjectUsers.Any())
            {
                var reloadedProject = await repository.GetProjectWithUsersAsync(project.Id, cancellationToken);
                if (reloadedProject is null)
                {
                    return false;
                }

                project = reloadedProject;
            }

            // Add user to project using domain method
            try
            {
                project.AddUser(userId, role, invitedBy, auditContext);

                // Save changes
                await repository.UpdateAsync(project, cancellationToken);
                await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                // Publish integration event for Auth domain synchronization
                var integrationEvent = new UserAddedToProjectIntegrationEvent(
                    userId,
                    userEmail,
                    userName,
                    project.Id,
                    project.Name,
                    project.BusinessIncubatorId,
                    role,
                    timeProvider.UtcNow);

                await mediator.Publish(integrationEvent, cancellationToken);

                return true;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already active"))
            {
                // User is already active in the project, consider it a success
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding user {UserId} to project {ProjectId}", userId, project.Id);
            return false;
        }
    }
}
