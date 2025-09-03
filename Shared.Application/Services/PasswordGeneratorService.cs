using System.Security.Cryptography;

namespace LinaSys.Shared.Application.Services;

/// <summary>
/// Implementation of password generator service for creating secure passwords.
/// </summary>
public class PasswordGeneratorService : IPasswordGeneratorService
{
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_-+=[]{}|;:,.<>?";

    /// <inheritdoc />
    public string GenerateTemporaryPassword(int length = 12)
    {
        return GeneratePassword(
            length: length,
            requireUppercase: true,
            requireLowercase: true,
            requireDigit: true,
            requireSpecialChar: true);
    }

    /// <inheritdoc />
    public string GeneratePassword(
        int length = 12,
        bool requireUppercase = true,
        bool requireLowercase = true,
        bool requireDigit = true,
        bool requireSpecialChar = true)
    {
        if (length < 8)
        {
            throw new ArgumentException("Password length must be at least 8 characters.", nameof(length));
        }

        var password = new List<char>();
        var allChars = string.Empty;

        // Add required character types and build the character pool
        if (requireUppercase)
        {
            password.Add(GetRandomChar(UppercaseChars));
            allChars += UppercaseChars;
        }

        if (requireLowercase)
        {
            password.Add(GetRandomChar(LowercaseChars));
            allChars += LowercaseChars;
        }

        if (requireDigit)
        {
            password.Add(GetRandomChar(DigitChars));
            allChars += DigitChars;
        }

        if (requireSpecialChar)
        {
            password.Add(GetRandomChar(SpecialChars));
            allChars += SpecialChars;
        }

        // If no character types are required, use all types
        if (string.IsNullOrEmpty(allChars))
        {
            allChars = UppercaseChars + LowercaseChars + DigitChars + SpecialChars;
        }

        // Fill the rest of the password with random characters
        var remainingLength = length - password.Count;
        for (int i = 0; i < remainingLength; i++)
        {
            password.Add(GetRandomChar(allChars));
        }

        // Shuffle the password to avoid predictable patterns
        return new string(password.OrderBy(_ => GetRandomInt()).ToArray());
    }

    private static char GetRandomChar(string chars)
    {
        return chars[GetRandomInt(0, chars.Length)];
    }

    private static int GetRandomInt(int minValue = 0, int maxValue = int.MaxValue)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var value = BitConverter.ToInt32(bytes, 0);

        // Convert to positive value and scale to the desired range
        var range = (long)maxValue - minValue;
        var scaled = Math.Abs(value) % range;
        return (int)(scaled + minValue);
    }
}
