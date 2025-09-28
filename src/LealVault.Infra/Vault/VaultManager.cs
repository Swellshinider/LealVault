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
    public event LogMessage? LogMessageEvent;

    private string? _vaultPath;

    /// <summary>
    /// If the vault is open
    /// </summary>
    public bool IsOpen { get; private set; } = false;

    /// <summary>
    /// The master password
    /// </summary>
    public string? MasterPassword { get; private set; }

    /// <summary>
    /// The vault data
    /// </summary>
    public VaultData? VaultData { get; private set; }

    /// <summary>
    /// Create a new vault
    /// </summary>
    public void Create(string vaultPath)
    {
        if (IsOpen)
        {
            LogMessageEvent?.Invoke("A vault is currently open, please close it before creating a new one.");
            return;
        }

        if (!IsPathValid(vaultPath))
            return;

        _vaultPath = vaultPath;
        using var _ = File.Create(_vaultPath);
        IsOpen = true;
        ReadData();
    }

    /// <summary>
    /// Open an existing vault
    /// </summary>
    public void Open(string vaultPath)
    {
        if (IsOpen)
        {
            LogMessageEvent?.Invoke("A vault is currently open, please close it before creating a new one.");
            return;
        }

        if (!IsPathValid(vaultPath))
            return;

        _vaultPath = vaultPath;
        IsOpen = true;
        ReadData();
    }

    /// <summary>
    /// Save the vault data to the file
    /// </summary>
    public void Save()
    {
        if (!IsOpen)
        {
            LogMessageEvent?.Invoke("No vault is currently open.");
            return;
        }

        if (VaultData is null)
        {
            LogMessageEvent?.Invoke("No vault data to save.");
            return;
        }

        if (string.IsNullOrEmpty(MasterPassword))
        {
            LogMessageEvent?.Invoke("No master password set.");
            return;
        }

        if (string.IsNullOrEmpty(_vaultPath))
        {
            LogMessageEvent?.Invoke("No vault path set.");
            return;
        }

        VaultCrypto.SaveEncrypted(_vaultPath, VaultData, MasterPassword);
        ReadData();
    }

    #region [ Util ]
    private void ReadData()
    {
        if (string.IsNullOrEmpty(MasterPassword))
        {
            LogMessageEvent?.Invoke("No master password set.");
            return;
        }

        if (string.IsNullOrEmpty(_vaultPath))
        {
            LogMessageEvent?.Invoke("No vault path set.");
            return;
        }

        VaultData = VaultCrypto.LoadDecrypted(_vaultPath, MasterPassword);

        if (VaultData is null)
        {
            LogMessageEvent?.Invoke("No vault data found.");
            VaultData = new();
            return;
        }
    }

    private bool IsPathValid(string path)
    {
        if (!Directory.Exists(path))
        {
            LogMessageEvent?.Invoke($"Unable to find directory {path}.");
            return false;
        }

        var fileName = Path.GetFileName(path);
        if (File.Exists(fileName))
        {
            LogMessageEvent?.Invoke($"A vault already exists at {path}.");
            return false;
        }

        if (!fileName.EndsWith(".lv"))
        {
            LogMessageEvent?.Invoke($"Your vault file should have a .lv extension.");
            return false;
        }

        return true;
    }
    #endregion
}