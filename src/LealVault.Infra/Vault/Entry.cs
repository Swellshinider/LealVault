using System.Text;
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

    /// <summary>
    /// Compares two entries and returns a string with the differences
    /// </summary>
    public void PrintDiff(Entry updatedEntry)
    {
        if (Name != updatedEntry.Name)
            PrintFormatted("Name", Name, updatedEntry.Name);

        if (Email != updatedEntry.Email)
            PrintFormatted("Email", Email, updatedEntry.Email);

        if (Password != updatedEntry.Password)
            PrintFormatted("Password", Password, updatedEntry.Password);

        if (Tag != updatedEntry.Tag)
            PrintFormatted("Tag", Tag, updatedEntry.Tag);

        if (Notes != updatedEntry.Notes)
            PrintFormatted("Notes", Notes, updatedEntry.Notes);
    }

    private static void PrintFormatted(string property, string? before, string? after)
    {
        $"    {property}".Write();
        $": {before ?? "<empty>"}".Write(ConsoleColor.Yellow);
        " -> ".Write();
        $"{after ?? "<empty>"}".WriteLine(ConsoleColor.Green);
    }
}