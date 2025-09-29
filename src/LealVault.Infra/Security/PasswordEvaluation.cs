namespace LealVault.Infra.Security;

/// <summary>
/// Password evaluation result
/// </summary>
public class PasswordEvaluation
{
    /// <summary>
    /// Password score
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Password category
    /// </summary>
    public PasswordCategory Category { get; set; }

    /// <summary>
    /// Password entropy in bits
    /// </summary>
    public double EntropyBits { get; set; }

    /// <summary>
    /// Warnings about password
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Suggestions about password
    /// </summary>
    public List<string> Suggestions { get; set; } = [];
}