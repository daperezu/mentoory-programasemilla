using Microsoft.AspNetCore.Identity;

namespace LinaSys.Sandbox.Misc;

public class PasswordHasher
{
    public void HashPassword()
    {
        var passwordHasher = new PasswordHasher<string>();
        string hashedPassword = passwordHasher.HashPassword("admin@linasys.com", "linasys123");
        Console.WriteLine($"Hashed Password: {hashedPassword}");
    }
}
