using FluentValidation;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Queries;

/// <summary>
/// Query to retrieve users by their email addresses.
/// </summary>
/// <param name="Emails">The list of email addresses to search for.</param>
public record GetUsersByEmailsQuery(IEnumerable<string> Emails) : IBaseRequest<UsersByEmailsResult>;

/// <summary>
/// Result containing users found by email and emails not found.
/// </summary>
/// <param name="ExistingUsers">Dictionary of email to user for existing users.</param>
/// <param name="NonExistentEmails">List of emails that don't have corresponding users.</param>
public record UsersByEmailsResult(
    Dictionary<string, User> ExistingUsers,
    List<string> NonExistentEmails);

/// <summary>
/// Validator for the GetUsersByEmailsQuery.
/// </summary>
public class GetUsersByEmailsQueryValidator : AbstractValidator<GetUsersByEmailsQuery>
{
    public GetUsersByEmailsQueryValidator()
    {
        RuleFor(x => x.Emails)
            .NotNull()
            .WithMessage("La lista de emails no puede ser nula.")
            .Must(emails => emails is not null && emails.Any())
            .WithMessage("La lista de emails no puede estar vacía.");

        RuleForEach(x => x.Emails)
            .NotEmpty()
            .WithMessage("El email no puede estar vacío.")
            .EmailAddress()
            .WithMessage("El formato del email debe ser válido.")
            .MaximumLength(256)
            .WithMessage("El email no puede exceder 256 caracteres.");

        RuleFor(x => x.Emails)
            .Must(emails => emails is null || emails.Count() <= 100)
            .WithMessage("No se pueden consultar más de 100 emails a la vez.");
    }
}

/// <summary>
/// Handler for the GetUsersByEmailsQuery.
/// </summary>
public class GetUsersByEmailsQueryHandler(
    IAuthRepository authRepository,
    ILogger<GetUsersByEmailsQueryHandler> logger)
    : BaseCommandHandler<GetUsersByEmailsQuery, UsersByEmailsResult>
{
    /// <summary>
    /// Handles the GetUsersByEmailsQuery.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the users found by email and emails not found.</returns>
    public override async Task<Result<UsersByEmailsResult>> Handle(GetUsersByEmailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Normalize and deduplicate emails
            var normalizedEmails = request.Emails
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => email.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (!normalizedEmails.Any())
            {
                logger.LogWarning("GetUsersByEmailsQuery called with no valid emails");
                return Success(new UsersByEmailsResult(
                    ExistingUsers: [],
                    NonExistentEmails: []));
            }

            logger.LogInformation("Searching for {EmailCount} users by email", normalizedEmails.Count);

            // Query all users with matching emails in a single database call
            var existingUserDict = await authRepository.GetUsersByEmailsAsync(normalizedEmails, cancellationToken);

            // Create set of found emails for comparison
            var foundEmails = new HashSet<string>(existingUserDict.Keys, StringComparer.OrdinalIgnoreCase);

            // Find emails that don't have corresponding users
            var nonExistentEmails = request.Emails
                .Where(email => !string.IsNullOrWhiteSpace(email) && !foundEmails.Contains(email.Trim()))
                .Select(email => email.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            logger.LogInformation(
                "Found {ExistingCount} existing users and {NonExistentCount} non-existent emails",
                existingUserDict.Count,
                nonExistentEmails.Count);

            var result = new UsersByEmailsResult(
                ExistingUsers: existingUserDict,
                NonExistentEmails: nonExistentEmails);

            return Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users by emails");
            return Failure(
                ResultErrorCodes.Auth_QueryFailed,
                (nameof(request.Emails), "Error interno al consultar usuarios por email."));
        }
    }
}
