namespace LealVault.Infra.Repl;

/// <summary>
/// Repl result.
/// </summary>
internal sealed class ReplResult
{
    public static ReplResult Empty { get; } = new(string.Empty, false, false);
    public static ReplResult Enter { get; } = new(string.Empty, false, true);

    public string Value { get; }
    public bool Override { get; }
    public bool Return { get; }

    public ReplResult(string value, bool overrideLine = false, bool @return = false)
    {
        Value = value ?? string.Empty;
        Override = overrideLine;
        Return = @return;
    }
}