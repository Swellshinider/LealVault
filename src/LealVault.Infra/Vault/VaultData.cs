namespace LealVault.Infra.Vault;

/// <summary>
/// Vault data class
/// </summary>
[Serializable]
public sealed class VaultData
{   
    /// <summary>
    /// Entries list
    /// </summary>
    public List<Entry> Entries { get; set; } = [];
}