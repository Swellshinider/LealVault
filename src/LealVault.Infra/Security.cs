using System.Security.Cryptography;
using System.Text;

namespace LealVault.Infra;

internal static class Security
{
    private static readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private static readonly int AlphabetSize = Alphabet.Length;
    private static readonly int MinIdLength = 5; // floor to avoid shorter ids

    /// <summary>
    /// Derives a key from a password using Argon2id.
    /// </summary>
    internal static byte[] DeriveKeyArgon2(string password, byte[] salt, int iterations, int memoryKiB, int parallelism)
    {
        if (salt == null || salt.Length == 0)
            throw new ArgumentException("salt required for key derivation");

        var pwBytes = Encoding.UTF8.GetBytes(password);

        try
        {
            var argon = new Konscious.Security.Cryptography.Argon2id(pwBytes)
            {
                Salt = salt,
                Iterations = iterations,
                MemorySize = memoryKiB,
                DegreeOfParallelism = parallelism
            };

            return argon.GetBytes(32);
        }
        finally
        {
            // free sensitive data
            for (int i = 0; i < pwBytes.Length; i++) pwBytes[i] = 0;
        }
    }

    /// <summary>
    /// Encrypts data using AES-GCM.
    /// </summary>
    internal static (byte[] nonce, byte[] combined) EncryptAesGcm(byte[] key, byte[] plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tagLen = 16;
        var ctLen = plaintext.Length;
        var ct = new byte[ctLen];
        var tag = new byte[tagLen];
        using var aes = new AesGcm(key, tagLen);
        aes.Encrypt(nonce, plaintext, ct, tag);
        var combined = new byte[ct.Length + tagLen];
        Buffer.BlockCopy(ct, 0, combined, 0, ct.Length);
        Buffer.BlockCopy(tag, 0, combined, ct.Length, tagLen);
        return (nonce, combined);
    }

    /// <summary>
    /// Decrypts data encrypted with AES-GCM.
    /// </summary>
    internal static byte[] DecryptAesGcm(byte[] key, byte[] nonce, byte[] combined)
    {
        var tagLen = 16;
        var ctLen = combined.Length - tagLen;
        var ct = new byte[ctLen];
        var tag = new byte[tagLen];
        Buffer.BlockCopy(combined, 0, ct, 0, ctLen);
        Buffer.BlockCopy(combined, ctLen, tag, 0, tagLen);
        var plain = new byte[ctLen];
        using var aes = new AesGcm(key, tagLen);
        aes.Decrypt(nonce, ct, tag, plain);
        return plain;
    }

    /// <summary>
    /// Generates a unique ID not present in the provided list, using a probabilistic approach to minimize collisions.
    /// </summary>
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