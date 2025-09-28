namespace LealVault.Infra.Commands;

/// <summary>
/// Represents the type of a command.
/// </summary>
internal enum CommandType
{
    Help,
    Exit,
    Create,
    Open,
    Save,
    Close,
    List
}