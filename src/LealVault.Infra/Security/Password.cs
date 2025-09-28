using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace LealVault.Infra.Security;

internal static partial class Password
{
    [GeneratedRegex("[a-z]")]
    private static partial Regex RegexLowercase();

    [GeneratedRegex("[A-Z]")]
    private static partial Regex RegexUppercase();

    [GeneratedRegex("[0-9]")]
    private static partial Regex RegexNumbers();

    [GeneratedRegex("[^A-Za-z0-9]")]
    private static partial Regex RegexAlphaNumeric();

    private static readonly string LOWERCASE = "abcdefghijklmnopqrstuvwxyz";
    private static readonly string UPPERCASE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string DIGITS = "0123456789";
    private static readonly string SPECIAL = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    private static readonly string[] LetterSequences =
    [
        "abcdefghijklmnopqrstuvwxyz",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "0123456789",
        "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?"
    ];

    private static readonly string[] CommonPasswords =
    [
        "111111",
        "12345",
        "123456",
        "1234567",
        "12345678",
        "123456789",

        "password",
        "sunshine",
        "iloveyou",
        "princess",
        "welcome",
        "admin",
        "login",
        "qwerty",
    ];

    private static readonly Dictionary<char, char> LeetSubistitutions = new()
    {
        {'0', 'o'}, {'1', 'l'}, {'3', 'e'}, {'4', 'a'},
        {'5', 's'}, {'7', 't'}, {'@', 'a'}, {'$', 's'}
    };

    private static int CharsetSize(string pw)
    {
        int size = 0;

        if (RegexLowercase().IsMatch(pw))
            size += 26;
        if (RegexUppercase().IsMatch(pw))
            size += 26;
        if (RegexNumbers().IsMatch(pw))
            size += 10;
        if (RegexAlphaNumeric().IsMatch(pw))
            size += 32;

        return Math.Max(1, size);
    }

    private static double BaseEntropy(string pw)
        => pw.Length * Math.Log2(CharsetSize(pw));

    private static int CountSequences(string password)
    {
        var found = 0;
        var lower = password.ToLower();

        foreach (var seq in LetterSequences)
        {
            for (var i = 0; i <= seq.Length - 3; i++)
            {
                if (lower.Contains(seq.Substring(i, 3)))
                    found++;
            }
        }

        return found;
    }

    private static double RepeatedCharPenalty(string pw)
    {
        int maxRun = 1, run = 1;

        for (int i = 1; i < pw.Length; i++)
        {
            if (pw[i] == pw[i - 1])
            {
                run++;
                maxRun = Math.Max(maxRun, run);
            }
            else
            {
                run = 1;
            }
        }

        return Math.Max(0, maxRun - 2) * 1.5;
    }

    private static double DictionaryPenalty(string pw)
    {
        string lower = pw.ToLower();
        double penalty = 0;

        if (CommonPasswords.Contains(lower))
            penalty += 40;

        foreach (string word in CommonPasswords)
        {
            if (word.Length >= 4 && lower.Contains(word))
                penalty += 15;
        }

        return penalty;
    }

    private static string LeetNormalize(string s)
        => new([.. s.ToLower().Select(c => LeetSubistitutions.TryGetValue(c, out char value) ? value : c)]);

    public static PasswordEvaluation EvaluatePassword(string pw, string? userHint = null)
    {
        if (string.IsNullOrEmpty(pw))
        {
            return new PasswordEvaluation
            {
                Score = 0,
                Category = PasswordCategory.VeryWeak,
                EntropyBits = 0
            };
        }

        var entropy = BaseEntropy(pw);
        var warnings = new List<string>();
        var suggestions = new List<string>();

        // Check for sequences
        var seqCount = CountSequences(pw);
        if (seqCount > 0)
        {
            entropy -= seqCount * 2.5;
            warnings.Add("contains sequence(s)");
            suggestions.Add("avoid sequences like 'abcd' or '1234'");
        }

        // Check for repeated characters
        var rep = RepeatedCharPenalty(pw);
        if (rep > 0)
        {
            entropy -= rep;
            warnings.Add("repeated runs");
            suggestions.Add("avoid long repeats");
        }

        // Check dictionary penalty
        var dictPen = DictionaryPenalty(pw);
        if (dictPen > 0)
        {
            entropy -= dictPen;
            warnings.Add("contains common word");
            suggestions.Add("avoid common words/passwords");
        }

        // Check leet speak
        var normalized = LeetNormalize(pw);
        if (CommonPasswords.Any(w => normalized.Contains(w)))
        {
            entropy -= 12;
            warnings.Add("possible leetspeak of common word");
            suggestions.Add("avoid predictable leet replacements");
        }

        // Check hint similarity
        if (!string.IsNullOrEmpty(userHint) && pw.Contains(userHint, StringComparison.CurrentCultureIgnoreCase))
        {
            entropy -= 20;
            warnings.Add("similar to username/email");
            suggestions.Add("don't include your name/email");
        }

        var regexes = new[]
        {
            RegexLowercase(),
            RegexUppercase(),
            RegexNumbers(),
            RegexAlphaNumeric()
        };
        var classes = regexes.Count(rx => rx.IsMatch(pw));

        if (classes >= 3 && pw.Length >= 10)
            entropy += 4;

        entropy = Math.Max(0, entropy);

        const double maxBits = 80;
        var score = Math.Min(100, (int)Math.Round(entropy / maxBits * 100));
        score = Math.Max(0, score);

        var category = score < 20 ? PasswordCategory.VeryWeak :
                       score < 40 ? PasswordCategory.Weak :
                       score < 60 ? PasswordCategory.Moderate :
                       score < 80 ? PasswordCategory.Strong :
                                    PasswordCategory.VeryStrong;

        if (suggestions.Count == 0 && pw.Length < 12)
            suggestions.Add("use at least 12 characters");

        return new PasswordEvaluation
        {
            Score = score,
            Category = category,
            EntropyBits = Math.Round(entropy * 100) / 100,
            Warnings = warnings,
            Suggestions = suggestions.Distinct().ToList()
        };
    }

    public static string GenerateSecurePassword(int length = 16, int maxAttempts = 100)
    {
        if (length < 12)
            throw new ArgumentException("Password length must be at least 12 characters for very strong passwords.");

        using (var rng = RandomNumberGenerator.Create())
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var password = GenerateRandomPassword(rng, length);
                var result = EvaluatePassword(password);

                // Only return if it's "Very strong" and has no warnings
                if (result.Category == PasswordCategory.VeryStrong && result.Warnings.Count == 0)
                    return password;
            }
        }

        throw new InvalidOperationException($"Failed to generate a very strong password after {maxAttempts} attempts.");
    }
    
    private static string GenerateRandomPassword(RandomNumberGenerator rng, int length)
    {
        // Ensure we have at least one character from each class
        var password = new List<char>
        {
            GetRandomChar(rng, LOWERCASE),
            GetRandomChar(rng, UPPERCASE),
            GetRandomChar(rng, DIGITS),
            GetRandomChar(rng, SPECIAL)
        };

        // Fill the rest with random characters from all classes
        string allChars = LOWERCASE + UPPERCASE + DIGITS + SPECIAL;
        for (int i = 4; i < length; i++)
        {
            password.Add(GetRandomChar(rng, allChars));
        }

        // Shuffle the password to avoid predictable patterns
        for (int i = password.Count - 1; i > 0; i--)
        {
            int j = GetRandomIndex(rng, i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password.ToArray());
    }

    private static char GetRandomChar(RandomNumberGenerator rng, string chars)
    {
        var index = GetRandomIndex(rng, chars.Length);
        return chars[index];
    }

    private static int GetRandomIndex(RandomNumberGenerator rng, int max)
    {
        var bytes = new byte[4];
        int result;

        do
        {
            rng.GetBytes(bytes);
            result = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF; // Remove sign bit
        } while (result >= int.MaxValue - (int.MaxValue % max)); // Avoid modulo bias

        return result % max;
    }
}