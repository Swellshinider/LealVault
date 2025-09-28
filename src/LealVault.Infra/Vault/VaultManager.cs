using LealVault.Infra.Security;

namespace LealVault.Infra.Vault;

/// <summary>
/// Vault manager class
/// </summary>
public sealed class VaultManager
{
    /// <summary>
    /// Log message event
    /// </summary>
    public delegate void LogMessage(string message);

    /// <summary>
    /// Log message event
    /// </summary>
    public event LogMessage? LogError;

    private string? _vaultPath;

    /// <summary>
    /// If the vault is open
    /// </summary>
    public bool IsOpen { get; private set; } = false;

    /// <summary>
    /// The master password
    /// </summary>
    public string? MasterPassword { get; set; }

    /// <summary>
    /// The vault data
    /// </summary>
    public VaultData? VaultData { get; private set; }

    /// <summary>
    /// Create a new vault
    /// </summary>
    public bool Create(string vaultPath)
    {
        if (IsOpen)
        {
            LogError?.Invoke("A vault is currently open, please close it before creating a new one.");
            return false;
        }

        if (!IsPathValid(vaultPath, out _vaultPath))
            return false;

        if (!TryCreateFile(_vaultPath))
            return false;

        VaultCrypto.SaveEncrypted(_vaultPath, new(), MasterPassword!);

        IsOpen = true;
        return true;
    }

    /// <summary>
    /// Open an existing vault
    /// </summary>
    public bool Open(string vaultPath)
    {
        if (IsOpen)
        {
            LogError?.Invoke("A vault is currently open, please close it before creating a new one.");
            return false;
        }

        if (!IsPathValid(vaultPath, out _vaultPath))
            return false;

        if (!ReadData())
            return false;

        IsOpen = true;
        return true;
    }

    /// <summary>
    /// Save the vault data to the file
    /// </summary>
    public bool Save()
    {
        if (!IsOpen)
        {
            LogError?.Invoke("No vault is currently open.");
            return false;
        }

        if (VaultData is null)
        {
            LogError?.Invoke("No vault data to save.");
            return false;
        }

        if (string.IsNullOrEmpty(MasterPassword))
        {
            LogError?.Invoke("No master password set.");
            return false;
        }

        if (string.IsNullOrEmpty(_vaultPath))
        {
            LogError?.Invoke("No vault path set.");
            return false;
        }

        VaultCrypto.SaveEncrypted(_vaultPath, VaultData, MasterPassword);
        return ReadData();
    }

    #region [ Util ]
    private bool TryCreateFile(string path)
    {
        try
        {
            if (File.Exists(_vaultPath))
            {
                LogError?.Invoke($"A vault already exists at {_vaultPath}.");
                return false;
            }

            using var _ = File.Create(path);
            return true;
        }
        catch (Exception e)
        {
            LogError?.Invoke($"Unable to create file {path}: {e.Message}.");
            return false;
        }
    }

    private bool ReadData()
    {
        if (string.IsNullOrEmpty(MasterPassword))
        {
            LogError?.Invoke("No master password set.");
            return false;
        }

        if (string.IsNullOrEmpty(_vaultPath))
        {
            LogError?.Invoke("No vault path set.");
            return false;
        }

        VaultData = VaultCrypto.LoadDecrypted(_vaultPath, MasterPassword);

        if (VaultData is null)
        {
            LogError?.Invoke("No vault data found.");
            VaultData = new();
            return false;
        }

        return true;
    }

    private bool IsPathValid(string vaultPath, out string fullPath)
    {
        fullPath = string.Empty;

        // Expand ~
        if (vaultPath.StartsWith('~'))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            vaultPath = Path.Combine(home, vaultPath.TrimStart('~').TrimStart(Path.DirectorySeparatorChar));
        }

        // Normalize
        var path = Path.GetFullPath(vaultPath);
        var directory = Path.GetDirectoryName(path);

        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            LogError?.Invoke($"Unable to find directory {directory}.");
            return false;
        }

        var fileName = Path.GetFileName(path);
        if (!fileName.EndsWith(".lv"))
        {
            LogError?.Invoke("Your vault file should have a .lv extension.");
            return false;
        }

        fullPath = path;
        return true;
    }
    #endregion
}