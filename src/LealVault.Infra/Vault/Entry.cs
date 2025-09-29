using LealVault.Infra.Security;

namespace LealVault.Infra.Vault;

/// <summary>
/// Entry class
/// </summary>
[Serializable]
public sealed record Entry
{
    private string? _password;

    /// <summary>
    /// Unique id
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Entry name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Entry tag
    /// </summary>
    public required string Tag { get; set; }

    /// <summary>
    /// Entry password
    /// </summary>
    public string Password
    {
        get => _password ?? "";
        set
        {
            PasswordEvaluation = Security.Password.EvaluatePassword(_password ?? "", Name);
            _password = value;
        }
    }

    /// <summary>
    /// Password strength
    /// </summary>
    public PasswordEvaluation? PasswordEvaluation { get; private set; }

    /// <summary>
    /// Entry email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Additional notes about entry
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public long Created { get; } = DateTime.Now.Ticks;

    /// <summary>
    /// Last modification date
    /// </summary>
    public required long Modified { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"{Id} | (Tag: {Tag}) - {Name}";
}