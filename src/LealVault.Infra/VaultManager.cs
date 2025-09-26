using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using LealVault.Infra.Models;

namespace LealVault.Infra;

/// <summary>
/// Manages the vault file, including loading, saving, locking, and autosaving.
/// </summary>
public partial class VaultManager : IDisposable
{
    // Internal file wrapper format
    private record FileWrapper(MetaInfo Meta, string Ciphertext);
    private record MetaInfo(int Version, string Argon2Params, string SaltB64, string NonceB64);

    // Config / state
    private FileStream? lockStream;
    private readonly object sync = new();

    // Key cache, this keeps derived key in memory for session
    private byte[]? cachedKey = null;
    private Timer? keyExpiryTimer;

    // Argon2 default params
    private const int Argon2Iterations = 3;
    private const int Argon2MemoryKiB = 65536;
    private const int Argon2Parallelism = 4;
    private Vault _vault = new();

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="VaultManager"/> class.
    /// </summary>
    public VaultManager() { }

    /// <summary>
    /// Path to the vault file.
    /// </summary>
    public string VaultPath { get; set; } = "";

    /// <summary>
    /// Indicates if there are unsaved changes in memory.
    /// </summary>
    public bool Dirty { get; private set; } = false;

    /// <summary>
    /// Opens the vault with the provided master password.
    /// </summary>
    /// <param name="masterPassword">
    /// The master password to unlock the vault.
    /// </param>
    /// <returns>
    /// Returns true if a new vault was created, false if an existing vault was opened.
    /// </returns>
    public bool Open(string masterPassword)
    {
        EnsureExclusiveLock();

        if (!File.Exists(VaultPath))
        {
            _vault = new Vault();

            Save(masterPassword);
            CacheKey(masterPassword, TimeSpan.FromMinutes(10));
            return true;
        }

        Load(masterPassword);
        CacheKey(masterPassword, TimeSpan.FromMinutes(10));
        return false;
    }

    private void EnsureExclusiveLock()
    {
        if (lockStream != null)
            return;

        // OpenOrCreate so we can use it as lock even if file doesn't exist yet 
        // FileShare.None grants exclusive access
        lockStream = new FileStream(VaultPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    /// <summary>
    /// Get all the entries in the vault.
    /// </summary>
    public IReadOnlyList<Entry> GetEntries() => _vault.Entries.AsReadOnly();

    /// <summary>
    /// Gets an entry by its ID.
    /// </summary>
    public Entry? GetEntry(string id) => _vault.Entries.Find(e => e.Id == id);

    /// <summary>
    /// Searches entries by title, tag, or details (case-insensitive).
    /// </summary>
    public List<Entry> SearchEntries(string query)
    {
        if (string.IsNullOrEmpty(query))
            throw new ArgumentNullException(nameof(query));

        return _vault.Entries.FindAll(e =>
            e.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
            (e.Tag ?? "").Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
            (e.Details ?? "").Contains(query, StringComparison.InvariantCultureIgnoreCase)
        );
    }

    /// <summary>
    /// Adds a new entry to the vault.
    /// </summary>
    public Entry AddEntry(Entry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        entry.Id = Security.GenerateUniqueId([.. GetEntries().Select(e => e.Id)]);
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = entry.CreatedAt;
        _vault.Entries.Add(entry);
        Dirty = true;
        return entry;
    }

    /// <summary>
    /// Removes an entry by its ID.
    /// </summary>
    public bool RemoveEntry(string id)
    {
        var removed = _vault.Entries.RemoveAll(e => e.Id == id) > 0;
        if (removed)
            Dirty = true;

        return removed;
    }

    /// <summary>
    /// Updates an existing entry in the vault.
    /// </summary>
    public void UpdateEntry(Entry updated)
    {
        ArgumentNullException.ThrowIfNull(updated);
        var idx = _vault.Entries.FindIndex(e => e.Id == updated.Id);

        if (idx == -1)
            throw new InvalidOperationException("Entry not found");

        updated.UpdatedAt = DateTime.UtcNow;
        _vault.Entries[idx] = updated;
        Dirty = true;
    }

    /// <summary>
    /// Saves the vault to disk using the provided master password.
    /// </summary>
    public void Save(string? masterPassword = null)
    {
        bool useCachedKey = false;
        if (string.IsNullOrEmpty(masterPassword))
        {
            if (cachedKey == null)
                throw new InvalidOperationException("No cached key available, please provide the master password");

            useCachedKey = true;
        }

        lock (sync)
        {
            var json = JsonSerializer.Serialize(_vault);
            var plain = Encoding.UTF8.GetBytes(json);

            // derive key
            var salt = RandomNumberGenerator.GetBytes(16);
            var key = useCachedKey ? cachedKey! : Security.DeriveKeyArgon2(masterPassword!, salt, Argon2Iterations, Argon2MemoryKiB, Argon2Parallelism);

            // encrypt
            var (nonce, combined) = Security.EncryptAesGcm(key, plain);

            // prepare wrapper
            var meta = new MetaInfo(
                Version: 1,
                Argon2Params: $"i={Argon2Iterations},m={Argon2MemoryKiB},p={Argon2Parallelism}",
                SaltB64: Convert.ToBase64String(salt),
                NonceB64: Convert.ToBase64String(nonce)
            );
            var wrapper = new FileWrapper(meta, Convert.ToBase64String(combined));
            var serialized = JsonSerializer.Serialize(wrapper, jsonSerializerOptions);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            // atomic write
            var tmp = VaultPath + ".tmp";
            File.WriteAllBytes(tmp, bytes);

            // flush to disk
            using (var fs = new FileStream(tmp, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                fs.Flush(true);
            }

            // optional backup
            if (File.Exists(VaultPath))
            {
                var bak = VaultPath + $".bak-{DateTime.UtcNow:yyyyMMddHHmmss}";
                File.Copy(VaultPath, bak, true);
            }

            // replace atomically if possible
            try
            {
                if (File.Exists(VaultPath))
                    File.Replace(tmp, VaultPath, null);
                else
                    File.Move(tmp, VaultPath);
            }
            catch
            {
                // fallback
                if (File.Exists(VaultPath)) File.Delete(VaultPath);
                File.Move(tmp, VaultPath);
            }

            // cleanup sensitive buffers
            Array.Clear(plain, 0, plain.Length);
            Array.Clear(key, 0, key.Length);
            Array.Clear(salt, 0, salt.Length);

            Dirty = false;
        }
    }

    /// <summary>
    /// Loads the vault from disk using the provided master password.
    /// </summary>
    private void Load(string masterPassword)
    {
        if (!File.Exists(VaultPath))
            throw new FileNotFoundException("Vault doesn't exist", VaultPath);

        lock (sync)
        {
            var text = File.ReadAllText(VaultPath, Encoding.UTF8);
            var wrapper = JsonSerializer.Deserialize<FileWrapper>(text)
                ?? throw new InvalidOperationException("Failed to parse vault file");

            var saltB64 = wrapper.Meta.SaltB64;
            var nonceB64 = wrapper.Meta.NonceB64;
            var ctB64 = wrapper.Ciphertext;

            var salt = string.IsNullOrEmpty(saltB64) ? [] : Convert.FromBase64String(saltB64);
            var nonce = Convert.FromBase64String(nonceB64);
            var combined = Convert.FromBase64String(ctB64);

            var key = Security.DeriveKeyArgon2(masterPassword, salt, Argon2Iterations, Argon2MemoryKiB, Argon2Parallelism);

            var plain = Security.DecryptAesGcm(key, nonce, combined);
            var json = Encoding.UTF8.GetString(plain);
            var obj = JsonSerializer.Deserialize<Vault>(json);

            // cleanup
            Array.Clear(plain, 0, plain.Length);
            Array.Clear(key, 0, key.Length);
            Array.Clear(salt, 0, salt.Length);

            _vault = obj ?? new Vault();
            Dirty = false;
        }
    }

    /// <summary>
    /// Caches the derived key in memory for the specified duration.
    /// </summary>
    private void CacheKey(string masterPassword, TimeSpan duration)
    {
        if (string.IsNullOrEmpty(masterPassword))
            throw new ArgumentNullException(nameof(masterPassword));

        lock (sync)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var key = Security.DeriveKeyArgon2(masterPassword, salt, Argon2Iterations, Argon2MemoryKiB, Argon2Parallelism);

            // limp previous if any
            ClearCachedKey();

            // store key
            cachedKey = key;

            // set expiry
            keyExpiryTimer = new Timer(_ => { ClearCachedKey(); }, null, duration, Timeout.InfiniteTimeSpan);

            // clear local salt variable
            Array.Clear(salt, 0, salt.Length);
        }
    }

    /// <summary>
    /// Clears the cached derived key from memory immediately.
    /// </summary>
    private void ClearCachedKey()
    {
        lock (sync)
        {
            if (cachedKey != null)
            {
                Array.Clear(cachedKey, 0, cachedKey.Length);
                cachedKey = null;
            }

            keyExpiryTimer?.Dispose();
            keyExpiryTimer = null;
        }
    }

    #region [ Dispose ]
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            ClearCachedKey();
            lockStream?.Dispose();
            lockStream = null;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}