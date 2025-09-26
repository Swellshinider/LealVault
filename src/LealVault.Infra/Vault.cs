using LealVault.Infra.Models;

namespace LealVault.Infra;

/// <summary>
/// Represents the data structure stored in the vault file.
/// </summary>
public class Vault
{
    /// <summary>
    /// List of entries in the vault.
    /// </summary>
    public List<Entry> Entries { get; set; } = [];
}