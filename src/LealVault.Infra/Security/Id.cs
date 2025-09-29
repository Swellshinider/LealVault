using System.Security.Cryptography;

namespace LealVault.Infra.Security;

internal static class Id
{
    private static readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private static readonly int AlphabetSize = Alphabet.Length;
    private static readonly int MinIdLength = 5; // floor to avoid shorter ids

    internal static string GenerateUniqueId(List<string> ids, double desiredCollisionProbability = 0.01, int maxRetriesPerLength = 10)
    {
        var k = Math.Max(1, ids.Count);
        var length = ComputeRequiredLength(k, desiredCollisionProbability);
        var maxRetryOverall = 100; // sanity to avoid infinite loop
        var overallAttempts = 0;

        while (true)
        {
            for (var attempt = 0; attempt < maxRetriesPerLength; attempt++)
            {
                var id = GenerateRandomId(length);
                if (!ids.Exists(e => e == id))
                    return id;
            }

            // If we got here, had too many collisions at this length (extremely unlikely),
            // increase length and retry.
            length++;
            overallAttempts++;

            if (overallAttempts >= maxRetryOverall)
                throw new InvalidOperationException("Failed to generate unique id after many attempts");
        }
    }

    private static int ComputeRequiredLength(int k, double p)
    {
        if (k < 2)
            return MinIdLength;

        if (p <= 0 || p >= 1)
            throw new ArgumentOutOfRangeException(nameof(p));

        // N_needed = k*(k-1) / (-2 * ln(1-p))
        var numerator = (double)k * (k - 1);
        var denom = -2.0 * Math.Log(1.0 - p);
        var Nneeded = numerator / denom;

        // L = ceil( log(Nneeded) / log(AlphabetSize) )
        var Ld = Math.Log(Nneeded, AlphabetSize);
        var L = (int)Math.Ceiling(Ld);

        if (L < MinIdLength)
            L = MinIdLength;

        return L;
    }

    private static string GenerateRandomId(int length)
    {
        var buf = new byte[length];
        RandomNumberGenerator.Fill(buf);
        var chars = new char[length];

        for (int i = 0; i < length; i++)
            chars[i] = Alphabet[buf[i] % AlphabetSize];
            
        return new string(chars);
    }
}