using LealVault.Infra.Commands;

namespace LealVault.Infra;

/// <summary>
/// Util methods
/// </summary>
public static class Util
{
    /// <summary>
    /// Check if string is null or empty
    /// </summary>
    public static bool IsNull(this string? str)
    {
        return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// Read password from console
    /// </summary>
    public static string ReadPassword()
    {
        var password = string.Empty;
        ConsoleKey key;

        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                password += keyInfo.KeyChar;
                Console.Write("*");
            }
        } while (key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    /// <summary>
    /// Convert command type to friendly name
    /// </summary>
    internal static string ToFriendlyName(this CommandType type)
        => (Enum.GetName(type.GetType(), type) ?? "").ToLower();
    
    /// <summary>
    /// Write text to console
    /// </summary>
    public static void Write(this string? text, ConsoleColor color = ConsoleColor.White)
    {
        if (text is null)
            return;

        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = previousColor;
    }

    /// <summary>
    /// Write text to console and move to next line
    /// </summary>
    public static void WriteLine(this string? text, ConsoleColor color = ConsoleColor.White)
    {
        if (text is null)
            return;

        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previousColor;
    }
}