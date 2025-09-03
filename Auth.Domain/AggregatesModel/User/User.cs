using LinaSys.Shared.Domain.SeedWork;
using Microsoft.AspNetCore.Identity;

namespace LinaSys.Auth.Domain.AggregatesModel.User;

/// <summary>
/// The user entity, where UserName is now the Person Identification Number (PIN).
/// </summary>
public class User : IdentityUser, IAggregateRoot
{
    public override string? UserName { get; set; } = default!; // Stores the PIN
}
