namespace LealVault.Infra.Models;

/// <summary>
/// Type of operation being performed on a vault entry
/// </summary>
public enum OperationType
{
    /// <summary>
    /// Type used for operations that don't support roolback, like a simple visualization
    /// </summary>
    None,

    /// <summary>
    /// Add a new entry
    /// </summary>
    Add,

    /// <summary>
    /// Update an existing entry
    /// </summary>
    Update,

    /// <summary>
    /// Delete an entry
    /// </summary>
    Delete,

    /// <summary>
    /// Exit command
    /// </summary>
    Exit
}