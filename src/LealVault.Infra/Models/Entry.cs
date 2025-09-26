
namespace LealVault.Infra.Models;

/// <summary>
/// Represents a single entry in the vault.
/// </summary>
[Serializable]
public record Entry
{
    /// <summary>
    /// Unique identifier for the entry.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Title of the entry.
    /// </summary>
    public string Title { get; set; } = "Unknown";

    /// <summary>
    /// Username associated with the entry.
    /// </summary>
    public string Username { get; set; } = "User";

    /// <summary>
    /// Password for the entry.
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// Tag for categorizing the entry.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// URL associated with the entry.
    /// </summary>
    public string? URL { get; set; }

    /// <summary>
    /// Email associated with the entry.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Additional details or notes for the entry.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp when the entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entry was updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Title;
}