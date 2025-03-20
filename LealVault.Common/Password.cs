using System.Security.Cryptography;

namespace LealVault.Common;

/// <summary>
/// Provides methods for generating secure passwords.
/// </summary>
public static class Password
{
    /// <summary>
    /// Generates a secure password
    /// </summary>
    /// <param name="length">The desired length of the password. Must be at least 4.</param>
    /// <returns>A secure password string.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified password length is less than 4.
    /// </exception>
    public static string GenerateSecurePassword(int length)
    {
        if (length < 4)
            throw new ArgumentException("Password length must be at least 4 to include all required character types.");

        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()-_=+[]{}|;:,.<>?";

        // Ensure one character from each required set
        var password = new char[length];

        using var rng = RandomNumberGenerator.Create();
        password[0] = uppercase[GetRandomInt(rng, uppercase.Length)];
        password[1] = lowercase[GetRandomInt(rng, lowercase.Length)];
        password[2] = digits[GetRandomInt(rng, digits.Length)];
        password[3] = special[GetRandomInt(rng, special.Length)];

        // Fill remaining characters from all sets combined
        var allChars = uppercase + lowercase + digits + special;

        for (int i = 4; i < length; i++)
            password[i] = allChars[GetRandomInt(rng, allChars.Length)];

        // Securely shuffle the array using Fisher-Yates algorithm
        ShuffleArray(rng, password);

        return new string(password);
    }

    /// <summary>
    /// Generates a random integer between 0 (inclusive) and a specified maximum (exclusive) using a cryptographic random number generator.
    /// </summary>
    /// <param name="rng">An instance of RandomNumberGenerator.</param>
    /// <param name="maxExclusive">The exclusive upper bound for the random number.</param>
    /// <returns>A random integer between 0 and maxExclusive - 1.</returns>
    private static int GetRandomInt(RandomNumberGenerator rng, int maxExclusive)
    {
        var uint32Buffer = new byte[4];

        while (true)
        {
            rng.GetBytes(uint32Buffer);

            uint random = BitConverter.ToUInt32(uint32Buffer, 0);
            long max = (1L + uint.MaxValue);
            long remainder = max % maxExclusive;

            if (random < max - remainder)
                return (int)(random % maxExclusive);
        }
    }

    /// <summary>
    /// Shuffles the elements of the specified character array in place using the Fisher-Yates algorithm and a cryptographic random number generator.
    /// </summary>
    /// <param name="rng">An instance of RandomNumberGenerator.</param>
    /// <param name="array">The character array to shuffle.</param>
    private static void ShuffleArray(RandomNumberGenerator rng, char[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = GetRandomInt(rng, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}