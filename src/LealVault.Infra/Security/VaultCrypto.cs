using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Konscious.Security.Cryptography;
using LealVault.Infra.Vault;

namespace LealVault.Infra.Security;

/// <summary>
/// Vault crypto class
/// </summary>
public static class VaultCrypto
{
    // File pack constants
    private static readonly byte[] MAGIC = Encoding.ASCII.GetBytes("VLT1"); // 4 bytes magic
    private const byte VERSION = 1;

    #region [ Argon2 params ]
    // time cost
    private const int ARGON2_ITERATIONS = 4;
    // 64 MiB
    private const int ARGON2_MEMORY_KiB = 65536;
    // threads
    private const int ARGON2_PARALLELISM = 2;
    // 256-bit AES key
    private const int KEY_LENGTH = 32;
    // bytes
    private const int SALT_SIZE = 16;
    // AES-GCM 96-bit nonce
    private const int NONCE_SIZE = 12;
    // AES-GCM tag size
    private const int TAG_SIZE = 16;
    #endregion

    /// <summary>
    /// Save (encrypt + write). File is locked during write.
    /// </summary>
    public static void SaveEncrypted(string path, VaultData vault, string masterPassword)
    {
        // Serialize the object to bytes
        var plain = JsonSerializer.SerializeToUtf8Bytes(vault);

        // Generate salt and nonce
        var salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        var nonce = RandomNumberGenerator.GetBytes(NONCE_SIZE);

        // Derive key from password using Argon2id
        var key = DeriveKeyFromPassword(masterPassword, salt, KEY_LENGTH);

        // Encrypt using AES-GCM
        var cipher = new byte[plain.Length];
        var tag = new byte[TAG_SIZE];

        using (var aes = new AesGcm(key, TAG_SIZE))
        {
            aes.Encrypt(nonce, plain, cipher, tag, BuildAad());
        }

        // Open file with exclusive lock while writing
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: false);

        // Packing the file:
        // [MAGIC(4)]
        // [VERSION(1)]
        // [salt length(1)]
        // [salt bytes]
        // [nonce length(1)]
        // [nonce bytes]
        // [tag length(1)]
        // [tag bytes]
        // [cipher length(4)]
        // [cipher bytes]

        bw.Write(MAGIC);                // [MAGIC(4)]
        bw.Write(VERSION);              // [VERSION(1)]

        bw.Write((byte)salt.Length);    // [salt length(1)]
        bw.Write(salt);                 // [salt bytes]

        bw.Write((byte)nonce.Length);   // [nonce length(1)]
        bw.Write(nonce);                // [nonce bytes]

        bw.Write((byte)tag.Length);     // [tag length(1)]
        bw.Write(tag);                  // [tag bytes]

        bw.Write(cipher.Length);        // [cipher length(4)]
        bw.Write(cipher);               // [cipher bytes]

        bw.Flush();
        fs.Flush(true); // ensure to persist

        // zero-out key in memory
        Array.Clear(key, 0, key.Length);
    }

    /// <summary>
    /// Load (decrypt + read). File is locked during read.
    /// </summary>
    public static VaultData? LoadDecrypted(string path, string masterPassword)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
        using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: false);

        // Unpacking the file:
        // [MAGIC(4)]
        // [VERSION(1)]
        // [salt length(1)]
        // [salt bytes]
        // [nonce length(1)]
        // [nonce bytes]
        // [tag length(1)]
        // [tag bytes]
        // [cipher length(4)]
        // [cipher bytes]

        var magic = br.ReadBytes(MAGIC.Length); // [MAGIC(4)]

        if (!AreEqual(magic, MAGIC)) // (magic mismatch)
            throw new InvalidDataException("This is not a vault file.");

        var version = br.ReadByte();            // [VERSION(1)]

        if (version != VERSION)
            throw new InvalidDataException($"Unsupported file version: {version}");

        var saltLen = br.ReadByte();            // [salt length(1)]
        var salt = br.ReadBytes(saltLen);       // [salt bytes]

        var nonceLen = br.ReadByte();           // [nonce length(1)]
        var nonce = br.ReadBytes(nonceLen);     // [nonce bytes]

        var tagLen = br.ReadByte();             // [tag length(1)]
        var tag = br.ReadBytes(tagLen);         // [tag bytes]

        var cipherLen = br.ReadInt32();         // [cipher length(4)]
        var cipher = br.ReadBytes(cipherLen);   // [cipher bytes]

        // Derive key
        var key = DeriveKeyFromPassword(masterPassword, salt, KEY_LENGTH);

        // Decrypt
        var plain = new byte[cipher.Length];
        try
        {
            using var aes = new AesGcm(key, tagLen);
            aes.Decrypt(nonce, cipher, tag, plain, BuildAad());
        }
        catch (CryptographicException)
        {
            throw new UnauthorizedAccessException("Incorrect password");
        }
        finally
        {
            Array.Clear(key, 0, key.Length);
        }

        return JsonSerializer.Deserialize<VaultData>(plain);
    }

    private static byte[] BuildAad()
    {
        // Associated data can bind metadata (like MAGIC/VERSION) into auth tag
        // so any tampering of those fields will break decryption.
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
        bw.Write(MAGIC);
        bw.Write(VERSION);
        bw.Flush();
        return ms.ToArray();
    }

    private static byte[] DeriveKeyFromPassword(string password, byte[] salt, int keyLen)
    {
        // Konscious implementation: Argon2id
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            Iterations = ARGON2_ITERATIONS,
            MemorySize = ARGON2_MEMORY_KiB,       // KiB
            DegreeOfParallelism = ARGON2_PARALLELISM
        };

        return argon2.GetBytes(keyLen);
    }

    private static bool AreEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        var diff = 0;
        for (var i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];

        return diff == 0;
    }
}