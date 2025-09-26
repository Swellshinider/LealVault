namespace LealVault.Infra.Models;

/// <summary>
/// Represents an operation to be performed on the vault.
/// </summary>
public class Operation
{
    /// <summary>
    /// Creates a new operation with the specified type and data.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    public Operation(OperationType type, object data)
    {
        Type = type;
        Data = data;
    }

    /// <summary>
    /// Operation type
    /// </summary>
    public OperationType Type { get; }

    /// <summary>
    /// Operation data
    /// </summary>
    public object Data { get; }
}