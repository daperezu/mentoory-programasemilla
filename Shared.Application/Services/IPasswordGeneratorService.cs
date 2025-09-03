namespace LinaSys.Shared.Application.Services;

/// <summary>
/// Service for generating secure passwords that meet complexity requirements.
/// </summary>
public interface IPasswordGeneratorService
{
    /// <summary>
    /// Generates a temporary password that meets ASP.NET Identity requirements.
    /// </summary>
    /// <param name="length">The desired length of the password (minimum 8).</param>
    /// <returns>A secure temporary password.</returns>
    string GenerateTemporaryPassword(int length = 12);

    /// <summary>
    /// Generates a password with specific complexity requirements.
    /// </summary>
    /// <param name="length">The desired length of the password.</param>
    /// <param name="requireUppercase">Whether to require uppercase letters.</param>
    /// <param name="requireLowercase">Whether to require lowercase letters.</param>
    /// <param name="requireDigit">Whether to require digits.</param>
    /// <param name="requireSpecialChar">Whether to require special characters.</param>
    /// <returns>A secure password meeting the specified requirements.</returns>
    string GeneratePassword(
        int length = 12,
        bool requireUppercase = true,
        bool requireLowercase = true,
        bool requireDigit = true,
        bool requireSpecialChar = true);
}
